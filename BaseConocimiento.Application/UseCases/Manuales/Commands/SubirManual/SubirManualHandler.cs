using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.Processing;
using BaseConocimiento.Application.Interfaces.Storage;
using BaseConocimiento.Application.Interfaces.VectorStore;
using BaseConocimiento.Domain.Entities;
using MediatR;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.SubirManual
{
    public class SubirManualHandler : IRequestHandler<SubirManualCommand, SubirManualResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;
        private readonly IQdrantService _qdrantService;
        private readonly IPdfProcessingService _pdfProcessor;
        private readonly IEmbeddingService _embeddingService;

        public SubirManualHandler(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage,
            IQdrantService qdrantService,
            IPdfProcessingService pdfProcessor,
            IEmbeddingService embeddingService)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _qdrantService = qdrantService;
            _pdfProcessor = pdfProcessor;
            _embeddingService = embeddingService;
        }

        public async Task<SubirManualResponse> Handle(SubirManualCommand request, CancellationToken ct)
        {
            var response = new SubirManualResponse();

            await _unitOfWork.ExecuteStrategyAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(ct);

                try
                {
                    var manual = Manual.Crear(
                        request.Titulo, request.Categoria, request.SubCategoria,
                        request.Version, request.Descripcion, "TEMP",
                        request.NombreOriginal, request.UsuarioId, request.ArchivoStream.Length
                    );

                    await _unitOfWork.Manuales.AgregarAsync(manual, ct);
                    await _unitOfWork.SaveChangesAsync(ct);

                    request.ArchivoStream.Position = 0;
                    var rutaFinal = await _fileStorage.GuardarArchivoAsync(manual.Id, request.ArchivoStream, request.NombreOriginal);

                    var propRuta = typeof(Manual).GetProperty("RutaLocal");
                    propRuta?.SetValue(manual, rutaFinal);
                    await _unitOfWork.SaveChangesAsync(ct);

                    request.ArchivoStream.Position = 0;
                    var paginas = await _pdfProcessor.ExtraerTextoAsync(request.ArchivoStream);

                    var chunks = new List<VectorChunk>();
                    int i = 1;
                    foreach (var pag in paginas.Where(p => !string.IsNullOrWhiteSpace(p.Texto)))
                    {
                        var vector = await _embeddingService.GenerarEmbeddingAsync(pag.Texto);
                        chunks.Add(new VectorChunk
                        {
                            Vector = vector,
                            TextoOriginal = pag.Texto,
                            NumeroPagina = pag.NumeroPagina,
                            NumeroChunk = i++
                        });
                    }

                    if (chunks.Any())
                        await _qdrantService.AlmacenarVectoresAsync(manual.Id, chunks);

                    await _unitOfWork.CommitTransactionAsync(ct);

                    response.Exitoso = true;
                    response.ManualId = manual.Id;
                    response.ChunksProcesados = chunks.Count;
                    response.Mensaje = "Procesado correctamente";
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    response.Exitoso = false;
                    response.Mensaje = ex.Message;
                    throw;
                }
            });

            return response;
        }
    }
}
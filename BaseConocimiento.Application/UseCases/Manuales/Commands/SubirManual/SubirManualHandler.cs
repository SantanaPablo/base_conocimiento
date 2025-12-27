using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.Processing;
using BaseConocimiento.Application.Interfaces.Storage;
using BaseConocimiento.Application.Interfaces.VectorStore;
using BaseConocimiento.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.SubirManual
{
    public class SubirManualHandler : IRequestHandler<SubirManualCommand, SubirManualResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;
        private readonly IQdrantService _qdrantService;
        private readonly IPdfProcessingService _pdfProcessor;
        private readonly ITextProcessingService _textProcessor;
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger<SubirManualHandler> _logger;

        public SubirManualHandler(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage,
            IQdrantService qdrantService,
            IPdfProcessingService pdfProcessor,
            ITextProcessingService textProcessor,
            IEmbeddingService embeddingService,
            ILogger<SubirManualHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _qdrantService = qdrantService;
            _pdfProcessor = pdfProcessor;
            _textProcessor = textProcessor;
            _embeddingService = embeddingService;
            _logger = logger;
        }

        public async Task<SubirManualResponse> Handle(SubirManualCommand request, CancellationToken ct)
        {
            var response = new SubirManualResponse();

            // ✅ USAR ExecuteStrategyAsync en lugar de BeginTransactionAsync
            await _unitOfWork.ExecuteStrategyAsync(async () =>
            {
                // Ahora sí usar transacciones dentro del strategy
                await _unitOfWork.BeginTransactionAsync(ct);

                try
                {
                    // Validar tipo de archivo
                    var extension = Path.GetExtension(request.NombreOriginal).ToLowerInvariant();
                    if (extension != ".pdf" && extension != ".txt")
                    {
                        throw new InvalidOperationException("Solo se permiten archivos PDF o TXT");
                    }

                    _logger.LogInformation("Subiendo manual: {Titulo}, Tipo: {Extension}", request.Titulo, extension);

                    // 1. Crear entidad
                    var manual = Manual.Crear(
                        request.Titulo,
                        request.Categoria,
                        request.SubCategoria,
                        request.Version,
                        request.Descripcion,
                        "TEMP",
                        request.NombreOriginal,
                        request.UsuarioId,
                        request.ArchivoStream.Length
                    );

                    await _unitOfWork.Manuales.AgregarAsync(manual, ct);
                    await _unitOfWork.SaveChangesAsync(ct);

                    // 2. Guardar archivo físico
                    request.ArchivoStream.Position = 0;
                    var rutaFinal = await _fileStorage.GuardarArchivoAsync(
                        manual.Id,
                        request.ArchivoStream,
                        request.NombreOriginal
                    );

                    var propRuta = typeof(Manual).GetProperty("RutaLocal");
                    propRuta?.SetValue(manual, rutaFinal);
                    await _unitOfWork.SaveChangesAsync(ct);

                    // 3. Procesar según el tipo de archivo
                    request.ArchivoStream.Position = 0;
                    List<TextoExtraido> paginas;

                    if (extension == ".pdf")
                    {
                        _logger.LogInformation("Procesando PDF...");
                        paginas = await _pdfProcessor.ExtraerTextoAsync(request.ArchivoStream);
                    }
                    else
                    {
                        _logger.LogInformation("Procesando TXT...");
                        paginas = await _textProcessor.ExtraerTextoAsync(request.ArchivoStream);
                    }

                    _logger.LogInformation("Texto extraído: {Count} fragmentos", paginas.Count);

                    // 4. Generar embeddings
                    var chunks = new List<VectorChunk>();
                    int chunkNumber = 1;

                    foreach (var pag in paginas.Where(p => !string.IsNullOrWhiteSpace(p.Texto)))
                    {
                        try
                        {
                            _logger.LogDebug("Generando embedding {Current}/{Total}", chunkNumber, paginas.Count);

                            var vector = await _embeddingService.GenerarEmbeddingAsync(pag.Texto);

                            chunks.Add(new VectorChunk
                            {
                                Vector = vector,
                                TextoOriginal = pag.Texto,
                                NumeroPagina = pag.NumeroPagina,
                                NumeroChunk = chunkNumber,
                                Categoria = manual.Categoria,
                                Titulo = manual.Titulo
                            });

                            chunkNumber++;

                            // Delay opcional cada 5 embeddings
                            if (chunkNumber % 5 == 0)
                            {
                                _logger.LogDebug("Esperando 2s para evitar rate limit...");
                                await Task.Delay(2000, ct);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al generar embedding para chunk {Chunk}", chunkNumber);
                        }
                    }

                    _logger.LogInformation("Embeddings generados: {Count} chunks", chunks.Count);

                    // 5. Almacenar en Qdrant
                    if (chunks.Any())
                    {
                        await _qdrantService.AlmacenarVectoresAsync(manual.Id, chunks);
                    }

                    await _unitOfWork.CommitTransactionAsync(ct);

                    response.Exitoso = true;
                    response.ManualId = manual.Id;
                    response.ChunksProcesados = chunks.Count;
                    response.Mensaje = $"Manual procesado correctamente. {chunks.Count} fragmentos indexados.";

                    _logger.LogInformation("Manual subido exitosamente: {ManualId}", manual.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al subir manual");
                    await _unitOfWork.RollbackTransactionAsync(ct);

                    response.Exitoso = false;
                    response.Mensaje = $"Error: {ex.Message}";
                    throw; // Re-lanzar para que ExecuteStrategyAsync lo maneje
                }
            });

            return response;
        }
    }
}
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

            await _unitOfWork.ExecuteStrategyAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(ct);

                try
                {
              
                    var extension = Path.GetExtension(request.NombreOriginal).ToLowerInvariant();
                    if (extension != ".pdf" && extension != ".txt")
                    {
                        throw new InvalidOperationException("Solo se permiten archivos PDF o TXT");
                    }

                 
                    var categoria = await _unitOfWork.Categorias.ObtenerPorIdAsync(request.CategoriaId, ct);
                    if (categoria == null)
                    {
                        throw new InvalidOperationException("La categoría especificada no existe");
                    }

                   
                    var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(request.UsuarioId, ct);
                    if (usuario == null)
                    {
                        throw new InvalidOperationException("El usuario especificado no existe");
                    }

                    _logger.LogInformation("Subiendo manual: {Titulo}, Categoría: {Categoria}, Usuario: {Usuario}",
                        request.Titulo, categoria.Nombre, usuario.NombreCompleto);

                    //Crear entidad Manual
                    var manual = Manual.Crear(
                        request.Titulo,
                        request.CategoriaId,
                        request.Version,
                        request.Descripcion,
                        "TEMP",
                        request.NombreOriginal,
                        request.UsuarioId,
                        request.ArchivoStream.Length
                    );

                    await _unitOfWork.Manuales.AgregarAsync(manual, ct);
                    await _unitOfWork.SaveChangesAsync(ct);

                    //Guardar físico
                    request.ArchivoStream.Position = 0;
                    var rutaStorage = await _fileStorage.GuardarArchivoAsync(
                        manual.Id,
                        request.ArchivoStream,
                        request.NombreOriginal
                    );

                    //Actualizar ruta en la entidad
                    var propRuta = typeof(Manual).GetProperty("RutaStorage");
                    propRuta?.SetValue(manual, rutaStorage);
                    await _unitOfWork.SaveChangesAsync(ct);

                    _logger.LogInformation("Archivo guardado en: {Ruta}", rutaStorage);

                    //Procesar según el tipo de archivo
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

                    //Generar embeddings
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
                                Categoria = categoria.Nombre,
                                Titulo = manual.Titulo
                            });

                            chunkNumber++;

                            
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

                    // 6. Almacenar en Qdrant
                    if (chunks.Any())
                    {
                        await _qdrantService.AlmacenarVectoresAsync(manual.Id, chunks);
                    }

                    await _unitOfWork.CommitTransactionAsync(ct);

                    response.Exitoso = true;
                    response.ManualId = manual.Id;
                    response.ChunksProcesados = chunks.Count;
                    response.Mensaje = $"Manual '{manual.Titulo}' procesado correctamente. {chunks.Count} fragmentos indexados.";

                    _logger.LogInformation("Manual subido exitosamente: {ManualId} - {Titulo}", manual.Id, manual.Titulo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al subir manual: {Titulo}", request.Titulo);
                    await _unitOfWork.RollbackTransactionAsync(ct);

                    response.Exitoso = false;
                    response.Mensaje = $"Error: {ex.Message}";
                    throw;
                }
            });

            return response;
        }
    }
}
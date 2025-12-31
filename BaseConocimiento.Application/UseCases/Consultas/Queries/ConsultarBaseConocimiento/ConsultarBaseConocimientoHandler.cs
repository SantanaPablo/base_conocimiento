using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.VectorStore;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BaseConocimiento.Application.UseCases.Consultas.Queries.ConsultarBaseConocimiento
{
    public class ConsultarBaseConocimientoHandler
        : IRequestHandler<ConsultarBaseConocimientoQuery, ConsultarBaseConocimientoResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmbeddingService _embeddingService;
        private readonly IQdrantService _qdrantService;
        private readonly IChatCompletionService _chatService;
        private readonly ILogger<ConsultarBaseConocimientoHandler> _logger;

        public ConsultarBaseConocimientoHandler(
            IUnitOfWork unitOfWork,
            IEmbeddingService embeddingService,
            IQdrantService qdrantService,
            IChatCompletionService chatService,
            ILogger<ConsultarBaseConocimientoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _embeddingService = embeddingService;
            _qdrantService = qdrantService;
            _chatService = chatService;
            _logger = logger;
        }

        public async Task<ConsultarBaseConocimientoResponse> Handle(
            ConsultarBaseConocimientoQuery request,
            CancellationToken ct)
        {
            try
            {
               
                var embedding = await _embeddingService.GenerarEmbeddingAsync(request.Pregunta);

                var resultados = await _qdrantService.BuscarSimilaresAsync(
                    embedding,
                    request.TopK,
                    request.CategoriaId?.ToString()
                );

                if (!resultados.Any())
                {
                    return new ConsultarBaseConocimientoResponse
                    {
                        Exitoso = true,
                        Respuesta = "No encontré información en los manuales para esa consulta."
                    };
                }

                var manualesDict = new Dictionary<Guid, string>();
                foreach (var id in resultados.Select(r => r.ManualId).Distinct())
                {
                    var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(id, ct);
                    manualesDict[id] = manual?.Titulo ?? "Manual Desconocido";
                }

                // 4. Construir contexto para el LLM
                var contexto = string.Join("\n\n", resultados.Select(r =>
                    $"[Fuente: {manualesDict[r.ManualId]}, Pág: {r.NumeroPagina}]\n{r.TextoOriginal}"));

                var prompt = $"Usa el siguiente contexto para responder la pregunta.\n\nCONTEXTO:\n{contexto}\n\nPREGUNTA: {request.Pregunta}";

                var respuesta = await _chatService.GenerarRespuestaAsync(prompt);

                return new ConsultarBaseConocimientoResponse
                {
                    Exitoso = true,
                    Respuesta = respuesta,
                    Fuentes = resultados.Select(r => new FuenteConsultadaDto
                    {
                        ManualId = r.ManualId,
                        Titulo = manualesDict[r.ManualId],
                        NumeroPagina = r.NumeroPagina,
                        Relevancia = Math.Round(r.Score * 100, 2),
                        TextoFragmento = r.TextoOriginal.Length > 200 ? r.TextoOriginal[..200] + "..." : r.TextoOriginal
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ConsultarBaseConocimiento");
                return new ConsultarBaseConocimientoResponse { Exitoso = false, Mensaje = ex.Message };
            }
        }
    }
}
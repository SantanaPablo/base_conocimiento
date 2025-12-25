using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.VectorStore;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("--- PASO 1: Generando embedding para la pregunta: {Pregunta} ---", request.Pregunta);
                var preguntaEmbedding = await _embeddingService.GenerarEmbeddingAsync(request.Pregunta);
                _logger.LogInformation("Embedding generado con éxito.");

                _logger.LogInformation("--- PASO 2: Buscando similitudes en Qdrant ---");
                var resultados = await _qdrantService.BuscarSimilaresAsync(
                    preguntaEmbedding,
                    request.TopK > 0 ? request.TopK : 5,
                    null // Buscamos en todos los manuales o poner categoria
                );

                _logger.LogInformation("Qdrant devolvió {Count} resultados.", resultados.Count);

                if (!resultados.Any())
                {
                    _logger.LogWarning("No se encontraron fragmentos relevantes en Qdrant.");
                    return new ConsultarBaseConocimientoResponse
                    {
                        Exitoso = true,
                        Respuesta = "No encontré información relevante en la base de conocimiento para responder tu pregunta.",
                        Fuentes = new List<FuenteConsultada>()
                    };
                }

                _logger.LogInformation("--- PASO 3: Recuperando títulos de manuales desde PostgreSQL ---");
                var manualIds = resultados.Select(r => r.ManualId).Distinct().ToList();
                var manuales = new Dictionary<Guid, string>();

                foreach (var id in manualIds)
                {
                    var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(id, cancellationToken);
                    if (manual != null)
                    {
                        manuales[id] = manual.Titulo;
                    }
                }

                var contexto = string.Join("\n\n", resultados.Select((r, i) =>
                {
                    var titulo = manuales.ContainsKey(r.ManualId) ? manuales[r.ManualId] : "Desconocido";
                    return $"[Fragmento {i + 1} - {titulo}, Página {r.NumeroPagina}]\n{r.TextoOriginal}";
                }));

                var prompt = $@"Eres un asistente experto que responde preguntas basándote ÚNICAMENTE en la información proporcionada.

CONTEXTO DE LA BASE DE CONOCIMIENTO:
{contexto}

PREGUNTA DEL USUARIO:
{request.Pregunta}

INSTRUCCIONES:
- Responde SOLO con información que aparezca en el contexto proporcionado
- Si la respuesta no está en el contexto, di claramente que no tienes esa información
- Cita la fuente (manual y página) cuando sea relevante
- Sé claro, preciso y profesional

RESPUESTA:";

                _logger.LogInformation("--- PASO 4: Solicitando respuesta al LLM (Gemini/Chat) ---");
                var respuesta = await _chatService.GenerarRespuestaAsync(prompt);
                _logger.LogInformation("Respuesta generada correctamente por el LLM.");

                var fuentes = resultados.Select(r => new FuenteConsultada
                {
                    ManualId = r.ManualId,
                    Titulo = manuales.ContainsKey(r.ManualId) ? manuales[r.ManualId] : "Desconocido",
                    NumeroPagina = r.NumeroPagina,
                    Relevancia = Math.Round(r.Score * 100, 2),
                    TextoFragmento = r.TextoOriginal.Length > 200
                        ? r.TextoOriginal.Substring(0, 200) + "..."
                        : r.TextoOriginal
                }).ToList();

                return new ConsultarBaseConocimientoResponse
                {
                    Exitoso = true,
                    Respuesta = respuesta,
                    Fuentes = fuentes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCURRIÓ UN ERROR EN EL HANDLER DE CONSULTA: {Message}", ex.Message);

                return new ConsultarBaseConocimientoResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error al consultar: {ex.Message}. Revisa los logs del servidor.",
                    Fuentes = new List<FuenteConsultada>()
                };
            }
        }
    }
}
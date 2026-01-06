using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Application.Interfaces.Conversation;
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.VectorStore;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace BaseConocimiento.Application.UseCases.Consultas.Commands.ConsultarConConversacion
{
    public class ConsultarConConversacionHandler
       : IRequestHandler<ConsultarConConversacionCommand, ConsultarConConversacionResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmbeddingService _embeddingService;
        private readonly IQdrantService _qdrantService;
        private readonly IChatCompletionService _chatService;
        private readonly IConversationService _conversationService;
        private readonly ILogger<ConsultarConConversacionHandler> _logger;

        public ConsultarConConversacionHandler(
            IUnitOfWork unitOfWork,
            IEmbeddingService embeddingService,
            IQdrantService qdrantService,
            IChatCompletionService chatService,
            IConversationService conversationService,
            ILogger<ConsultarConConversacionHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _embeddingService = embeddingService;
            _qdrantService = qdrantService;
            _chatService = chatService;
            _conversationService = conversationService;
            _logger = logger;
        }

        public async Task<ConsultarConConversacionResponse> Handle(
            ConsultarConConversacionCommand request,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                //Obtener o crear conversación
                string conversacionId = request.ConversacionId;

                if (string.IsNullOrEmpty(conversacionId) ||
                    !await _conversationService.ExisteConversacionAsync(conversacionId))
                {
                    conversacionId = await _conversationService.CrearConversacionAsync(request.UsuarioId);
                    _logger.LogInformation("Nueva conversación: {ConversacionId}", conversacionId);
                }

                //Obtener historial
                var historial = await _conversationService.ObtenerUltimosMensajesAsync(conversacionId, 5);
                _logger.LogInformation("Historial: {Count} mensajes", historial.Count);

                //Generar embedding y buscar
                var embedding = await _embeddingService.GenerarEmbeddingAsync(request.Pregunta);
                var resultados = await _qdrantService.BuscarSimilaresAsync(
                    embedding,
                    request.TopK,
                    string.IsNullOrEmpty(request.Categoria) ? null : request.Categoria
                );

                if (!resultados.Any())
                {
                    return new ConsultarConConversacionResponse
                    {
                        Exitoso = true,
                        ConversacionId = conversacionId,
                        Respuesta = "No encontré información relevante.",
                        Fuentes = new List<FuenteConsultada>()
                    };
                }

                var manualIds = resultados.Select(r => r.ManualId).Distinct();
                var manuales = new Dictionary<Guid, string>();
                foreach (var id in manualIds)
                {
                    var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(id, cancellationToken);
                    manuales[id] = manual?.Titulo ?? "Desconocido";
                }
                var contexto = string.Join("\n\n", resultados.Select((r, i) =>
                {
                    var titulo = manuales.GetValueOrDefault(r.ManualId) ?? "Manual no encontrado";
                    return $"[Fuente {i + 1} - {titulo}, Pág. {r.NumeroPagina}]\n{r.TextoOriginal}";
                }));
                var instrucciones = new StringBuilder();
                instrucciones.AppendLine("Sos Inuzaru, técnico nivel 2 de IT. Tu única fuente de verdad es la DOCUMENTACIÓN TÉCNICA.");
                if (historial != null && historial.Any()) {
    instrucciones.AppendLine("- Estás en una charla activa. Si el usuario cambió de tema, avisale y priorizá el nuevo manual.");
} else {
    instrucciones.AppendLine("- Esta es una consulta nueva. No menciones conversaciones anteriores porque no existen.");
}

instrucciones.AppendLine("- Si la info no está en el manual, admitilo. No inventes archivos .exe.");

                var prompt = $@"### INSTRUCCIÓN DE SISTEMA: PRIORIDAD DE DATOS
Sos Inuzaru. Tu prioridad absoluta es la 'DOCUMENTACIÓN TÉCNICA' que sigue abajo. 
Si la pregunta actual cambia de tema respecto a lo que veníamos hablando antes, descartá el historial de inmediato. 
Prohibido mezclar pasos de diferentes sistemas.

### DOCUMENTACIÓN TÉCNICA (Información Real de Qdrant):
{contexto}

### PREGUNTA ACTUAL DEL COMPAÑERO:
{request.Pregunta}

### REGLAS DE RESPUESTA:
1. Respondé en español rioplatense (voseo).
2. Si la solución no está en la 'DOCUMENTACIÓN TÉCNICA', no inventes nada.
3. Usá el historial de mensajes SOLO si la pregunta es de seguimiento (ej: '¿y qué versión?', '¿quién es el dueño?'). Si es un tema nuevo, ignoralo.

RESPUESTA:";

                //Generar respuesta con historial
                var respuesta = await _chatService.GenerarRespuestaConHistorialAsync(prompt, historial);

                //Guardar en historial
                await _conversationService.AgregarMensajeAsync(conversacionId, "user", request.Pregunta);
                await _conversationService.AgregarMensajeAsync(conversacionId, "assistant", respuesta);

                //Preparar fuentes
                var fuentes = resultados.Select(r => new FuenteConsultada
                {
                    ManualId = r.ManualId,
                    Titulo = manuales[r.ManualId],
                    NumeroPagina = r.NumeroPagina,
                    Relevancia = Math.Round(r.Score * 100, 2),
                    TextoFragmento = r.TextoOriginal.Length > 200
                        ? r.TextoOriginal.Substring(0, 200) + "..."
                        : r.TextoOriginal
                }).ToList();

                sw.Stop();
                _logger.LogInformation("Consulta con conversación completada en {Ms}ms", sw.ElapsedMilliseconds);

                return new ConsultarConConversacionResponse
                {
                    Exitoso = true,
                    ConversacionId = conversacionId,
                    Respuesta = respuesta,
                    Fuentes = fuentes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en consulta con conversación");
                return new ConsultarConConversacionResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error: {ex.Message}",
                    Fuentes = new List<FuenteConsultada>()
                };
            }
        }
    }
}

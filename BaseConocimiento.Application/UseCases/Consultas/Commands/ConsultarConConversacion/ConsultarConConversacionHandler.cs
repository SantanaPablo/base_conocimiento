using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Application.Interfaces.Conversation;
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.VectorStore;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    _logger.LogInformation("🆕 Nueva conversación: {ConversacionId}", conversacionId);
                }

                //Obtener historial
                var historial = await _conversationService.ObtenerUltimosMensajesAsync(conversacionId, 10);
                _logger.LogInformation("📖 Historial: {Count} mensajes", historial.Count);

                //Generar embedding y buscar
                var embedding = await _embeddingService.GenerarEmbeddingAsync(request.Pregunta);
                var resultados = await _qdrantService.BuscarSimilaresAsync(
                    embedding,
                    request.TopK,
                    request.Categoria
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

                //Obtener títulos de manuales
                var manualIds = resultados.Select(r => r.ManualId).Distinct();
                var manualTasks = manualIds.Select(async id =>
                {
                    var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(id, cancellationToken);
                    return (id, manual?.Titulo ?? "Desconocido");
                });
                var manuales = (await Task.WhenAll(manualTasks)).ToDictionary(x => x.id, x => x.Item2);

                //Construir contexto
                var contexto = string.Join("\n\n", resultados.Select((r, i) =>
                {
                    var titulo = manuales[r.ManualId];
                    return $"[Fuente {i + 1} - {titulo}, Pág. {r.NumeroPagina}]\n{r.TextoOriginal}";
                }));

                var prompt = $@"Eres un asistente técnico que responde basándose ÚNICAMENTE en el contexto proporcionado.

CONTEXTO DE LOS MANUALES:
{contexto}

PREGUNTA ACTUAL:
{request.Pregunta}

INSTRUCCIONES:
- Responde SOLO con información del contexto
- Si mencionas algo del historial, hazlo brevemente
- Sé claro y profesional
- Cita las fuentes cuando sea relevante

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
                _logger.LogInformation("✅ Consulta con conversación completada en {Ms}ms", sw.ElapsedMilliseconds);

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

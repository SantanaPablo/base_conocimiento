using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Application.Interfaces.Conversation;
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.VectorStore;
using BaseConocimiento.Domain.Entities;
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

                // GENERAR EMBEDDING DE LA PREGUNTA ACTUAL
                _logger.LogDebug("Generando embedding para: {Pregunta}", request.Pregunta);
                var embeddingActual = await _embeddingService.GenerarEmbeddingAsync(request.Pregunta);

                // Buscar en Qdrant
                var resultados = await _qdrantService.BuscarSimilaresAsync(
                    embeddingActual,
                    request.TopK,
                    string.IsNullOrEmpty(request.Categoria) ? null : request.Categoria
                );

                if (!resultados.Any())
                {
                    await _conversationService.AgregarMensajeAsync(conversacionId, "user", request.Pregunta);
                    await _conversationService.AgregarMensajeAsync(conversacionId, "assistant",
                        "No encontré info relevante. ¿Podés reformular la pregunta?");

                    return new ConsultarConConversacionResponse
                    {
                        Exitoso = true,
                        ConversacionId = conversacionId,
                        Respuesta = "No encontré info relevante. ¿Podés reformular la pregunta?",
                        Fuentes = new List<FuenteConsultada>()
                    };
                }

                // DETECTAR CAMBIO DE TEMA
                var historial = await _conversationService.ObtenerUltimosMensajesAsync(conversacionId, 5);
                bool esCambioTema = await DetectarCambioTemaAsync(historial, embeddingActual);

                if (esCambioTema)
                {
                    _logger.LogWarning("CAMBIO DE TEMA DETECTADO - Limpiando historial");
                    historial = new List<MensajeConversacion>();
                }
                else
                {
                    _logger.LogDebug("Mismo tema - Historial: {Count} mensajes", historial.Count);
                }

                // Obtener info de manuales
                var manualIds = resultados.Select(r => r.ManualId).Distinct();
                var manuales = new Dictionary<Guid, ManualInfo>();

                foreach (var id in manualIds)
                {
                    var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(id, cancellationToken);
                    if (manual != null)
                    {
                        manuales[id] = new ManualInfo
                        {
                            Titulo = manual.Titulo,
                            Categoria = manual.Categoria?.Nombre ?? "Sin categoría"
                        };
                    }
                }

                // Construir contexto enriquecido
                var contexto = ConstruirContexto(resultados, manuales);

                // Construir prompt
                var prompt = ConstruirPrompt(contexto, request.Pregunta, esCambioTema, manuales);

                // Generar respuesta CON historial
                var respuesta = await _chatService.GenerarRespuestaConHistorialAsync(prompt, historial);

                // Guardar en Redis
                await _conversationService.AgregarMensajeAsync(conversacionId, "user", request.Pregunta);
                await _conversationService.AgregarMensajeAsync(conversacionId, "assistant", respuesta);

                // Preparar fuentes
                var fuentes = resultados.Select(r => new FuenteConsultada
                {
                    ManualId = r.ManualId,
                    Titulo = manuales.ContainsKey(r.ManualId) ? manuales[r.ManualId].Titulo : "Desconocido",
                    NumeroPagina = r.NumeroPagina,
                    Relevancia = Math.Round(r.Score * 100, 2),
                    TextoFragmento = r.TextoOriginal.Length > 200
                        ? r.TextoOriginal.Substring(0, 200) + "..."
                        : r.TextoOriginal
                }).ToList();

                sw.Stop();
                _logger.LogInformation("Consulta completada en {Ms}ms", sw.ElapsedMilliseconds);

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
                _logger.LogError(ex, "Error en consulta");
                return new ConsultarConConversacionResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error: {ex.Message}",
                    Fuentes = new List<FuenteConsultada>()
                };
            }
        }

        private async Task<bool> DetectarCambioTemaAsync(
            List<MensajeConversacion> historial,
            float[] embeddingActual)
        {
            if (!historial.Any())
            {
                _logger.LogDebug("Sin historial - Primera consulta");
                return false;
            }

            // Obtener último mensaje del usuario
            var ultimoMensaje = historial
                .Where(m => m.Rol == "user")
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefault();

            if (ultimoMensaje == null)
            {
                _logger.LogDebug("No hay mensajes de usuario");
                return false;
            }

            try
            {
                // Generar embedding del mensaje anterior
                var embeddingAnterior = await _embeddingService.GenerarEmbeddingAsync(ultimoMensaje.Contenido);

                // Calcular similitud coseno
                var similitud = CalcularSimilitudCoseno(embeddingAnterior, embeddingActual);

                _logger.LogInformation("Similitud: {Similitud:F3} | Anterior: '{Anterior}' | Actual: '{Actual}'",
                    similitud,
                    ultimoMensaje.Contenido.Substring(0, Math.Min(50, ultimoMensaje.Contenido.Length)),
                    "pregunta actual");

                // cambio de tema
                return similitud < 0.5f;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al detectar cambio - Asumiendo mismo tema");
                return false;
            }
        }

        private float CalcularSimilitudCoseno(float[] a, float[] b)
        {
            if (a.Length != b.Length)
            {
                _logger.LogError("Vectores diferentes: {A} vs {B}", a.Length, b.Length);
                return 0f;
            }

            float dotProduct = 0f;
            float magA = 0f;
            float magB = 0f;

            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            magA = (float)Math.Sqrt(magA);
            magB = (float)Math.Sqrt(magB);

            if (magA == 0 || magB == 0) return 0f;

            return dotProduct / (magA * magB);
        }

        private string ConstruirContexto(List<ResultadoBusqueda> resultados, Dictionary<Guid, ManualInfo> manuales)
        {
            var sb = new StringBuilder();
            foreach (var (r, i) in resultados.Select((r, i) => (r, i)))
            {
                var manual = manuales.GetValueOrDefault(r.ManualId);
                sb.AppendLine($"[Fuente {i + 1}]");
                sb.AppendLine($"Manual: {manual?.Titulo ?? "Desconocido"}");
                sb.AppendLine($"Categoría: {manual?.Categoria ?? "N/A"}");
                sb.AppendLine($"Página: {r.NumeroPagina}");
                sb.AppendLine($"Score: {r.Score:F3}");
                sb.AppendLine("---");
                sb.AppendLine(r.TextoOriginal);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private string ConstruirPrompt(string contexto, string pregunta, bool esCambioTema, Dictionary<Guid, ManualInfo> manuales)
        {
            var instrucciones = new StringBuilder();
            instrucciones.AppendLine("Sos Inuzaru, técnico nivel 2 de IT. Tu única fuente es la DOCUMENTACIÓN TÉCNICA.");
            instrucciones.AppendLine();

            if (esCambioTema)
            {
                instrucciones.AppendLine(" CAMBIO DE TEMA DETECTADO:");
                instrucciones.AppendLine(" Olvidate del contexto anterior.");
                instrucciones.AppendLine(" Enfocate SOLO en la documentación actual.");
            }
            else
            {
                instrucciones.AppendLine(" CONTEXTO CONTINUO:");
                instrucciones.AppendLine(" Usá el historial para dar continuidad.");
                instrucciones.AppendLine(" Pero la documentación siempre manda.");
            }

            instrucciones.AppendLine();
            instrucciones.AppendLine("REGLAS:");
            instrucciones.AppendLine("1. Si no está en la doc, admitilo.");
            instrucciones.AppendLine("2. No inventes .exe, rutas o IPs.");
            instrucciones.AppendLine("3. No mezcles temas diferentes.");
            instrucciones.AppendLine("4. Hablá en argentino (voseo).");

            if (manuales.Any())
            {
                instrucciones.AppendLine();
                instrucciones.AppendLine(" MANUALES CONSULTADOS:");
                foreach (var m in manuales.Values.DistinctBy(x => x.Titulo))
                    instrucciones.AppendLine($"  • {m.Titulo} ({m.Categoria})");
            }

            return $@"
{instrucciones}

═══════════════════════════════════════
DOCUMENTACIÓN TÉCNICA:
═══════════════════════════════════════
{contexto}

═══════════════════════════════════════
PREGUNTA:
═══════════════════════════════════════
{pregunta}

RESPUESTA:";
        }

        private class ManualInfo
        {
            public string Titulo { get; set; }
            public string Categoria { get; set; }
        }
    }
}
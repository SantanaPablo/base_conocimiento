using BaseConocimiento.Application.Interfaces.Conversation;
using BaseConocimiento.Application.UseCases.Consultas.Commands.ConsultarConConversacion;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BaseConocimiento.API.DTOs.Conversation;

namespace BaseConocimiento.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversacionesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConversationService _conversationService;
        private readonly ILogger<ConversacionesController> _logger;

        public ConversacionesController(
            IMediator mediator,
            IConversationService conversationService,
            ILogger<ConversacionesController> logger)
        {
            _mediator = mediator;
            _conversationService = conversationService;
            _logger = logger;
        }

        /// <summary>
        /// Crear una nueva conversación
        /// </summary>
        [HttpPost("crear")]
        public async Task<IActionResult> CrearConversacion([FromBody] CrearConversacionRequest request)
        {
            var conversacionId = await _conversationService.CrearConversacionAsync(
                request.UsuarioId ?? "anonimo"
            );

            return Ok(new { conversacionId });
        }

        /// <summary>
        /// Preguntar con contexto de conversación (RAG + Historial)
        /// </summary>
        //[HttpPost("preguntar")]
        //public async Task<IActionResult> PreguntarConConversacion([FromBody] PreguntarConversacionRequest request)
        //{
        //    var command = new ConsultarConConversacionCommand
        //    {
        //        Pregunta = request.Pregunta,
        //        ConversacionId = request.ConversacionId,
        //        UsuarioId = User.Identity?.Name ?? request.UsuarioId ?? "anonimo",
        //        Categoria = request.Categoria,
        //        TopK = request.TopK > 0 ? request.TopK : 5
        //    };

        //    var resultado = await _mediator.Send(command);

        //    if (resultado.Exitoso)
        //        return Ok(resultado);

        //    return BadRequest(resultado);
        //}

        /// <summary>
        /// Obtener historial de una conversación
        /// </summary>
        [HttpGet("{conversacionId}/historial")]
        public async Task<IActionResult> ObtenerHistorial(string conversacionId)
        {
            if (!await _conversationService.ExisteConversacionAsync(conversacionId))
                return NotFound(new { Mensaje = "Conversación no encontrada" });

            var historial = await _conversationService.ObtenerHistorialAsync(conversacionId);
            var metadata = await _conversationService.ObtenerMetadataAsync(conversacionId);

            return Ok(new
            {
                Metadata = metadata,
                Mensajes = historial
            });
        }

        /// <summary>
        /// Limpiar/resetear una conversación
        /// </summary>
        [HttpDelete("{conversacionId}")]
        public async Task<IActionResult> LimpiarConversacion(string conversacionId)
        {
            if (!await _conversationService.ExisteConversacionAsync(conversacionId))
                return NotFound(new { Mensaje = "Conversación no encontrada" });

            await _conversationService.LimpiarConversacionAsync(conversacionId);

            return Ok(new { Mensaje = "Conversación limpiada exitosamente" });
        }

        /// <summary>
        /// Obtener metadata de conversación
        /// </summary>
        [HttpGet("{conversacionId}/metadata")]
        public async Task<IActionResult> ObtenerMetadata(string conversacionId)
        {
            var metadata = await _conversationService.ObtenerMetadataAsync(conversacionId);

            if (metadata == null)
                return NotFound(new { Mensaje = "Conversación no encontrada" });

            return Ok(metadata);
        }
    }

    
}

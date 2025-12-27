using Microsoft.AspNetCore.Mvc;
using MediatR;
using BaseConocimiento.Application.UseCases.Manuales.Commands.SubirManual;
using BaseConocimiento.Application.UseCases.Manuales.Commands.EliminarManual;
using BaseConocimiento.Application.UseCases.Manuales.Commands.ActualizarEstadoManual;
using BaseConocimiento.Application.UseCases.Manuales.Commands.ActualizarManual;
using BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerManual;
using BaseConocimiento.Application.UseCases.Manuales.Queries.ListarManuales;
using BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerCategorias;
using BaseConocimiento.Domain.Enums;
using BaseConocimiento.API.DTOs.Manuales;

namespace BaseConocimiento.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ManualesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ManualesController> _logger;

        public ManualesController(IMediator mediator, ILogger<ManualesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Subir un nuevo manual a la base de conocimiento
        /// </summary>
        /// <param name="request">Datos del manual y archivo PDF</param>
        /// <returns>Información del manual creado</returns>
        [HttpPost]
        [RequestSizeLimit(104857600)] // 100 MB
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubirManual([FromForm] SubirManualRequest request)
        {
            _logger.LogInformation("Subiendo manual: {Titulo}", request.Titulo);

            if (request.Archivo == null || request.Archivo.Length == 0)
                return BadRequest(new { Mensaje = "Debe proporcionar un archivo PDF" });

            if (!request.Archivo.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Mensaje = "Solo se permiten archivos PDF" });

            using var stream = request.Archivo.OpenReadStream();

            var command = new SubirManualCommand
            {
                ArchivoStream = stream,
                NombreOriginal = request.Archivo.FileName,
                Titulo = request.Titulo,
                Categoria = request.Categoria,
                SubCategoria = request.SubCategoria ?? string.Empty,
                Version = request.Version ?? "v1.0",
                Descripcion = request.Descripcion ?? string.Empty,
                UsuarioId = User.Identity?.Name ?? "anonimo"
            };

            var resultado = await _mediator.Send(command);

            if (resultado.Exitoso)
            {
                _logger.LogInformation("Manual subido exitosamente: {ManualId}", resultado.ManualId);
                return Ok(resultado);
            }

            _logger.LogWarning("Error al subir manual: {Mensaje}", resultado.Mensaje);
            return BadRequest(resultado);
        }

        /// <summary>
        /// Listar manuales con filtros y paginación
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarManuales(
            [FromQuery] string categoria = null,
            [FromQuery] string subCategoria = null,
            [FromQuery] EstadoManual? estado = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamañoPagina = 20)
        {
            var query = new ListarManualesQuery
            {
                Categoria = categoria,
                SubCategoria = subCategoria,
                Estado = estado,
                Pagina = pagina,
                TamañoPagina = Math.Min(tamañoPagina, 100) // Máximo 100 por página
            };

            var resultado = await _mediator.Send(query);
            return Ok(resultado);
        }

        /// <summary>
        /// Obtener detalles de un manual específico
        /// </summary>
        /// <param name="id">ID del manual</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerManual(Guid id)
        {
            var query = new ObtenerManualQuery { ManualId = id };
            var resultado = await _mediator.Send(query);

            if (resultado == null)
                return NotFound(new { Mensaje = $"Manual {id} no encontrado" });

            return Ok(resultado);
        }

        /// <summary>
        /// Eliminar un manual de la base de conocimiento
        /// </summary>
        /// <param name="id">ID del manual a eliminar</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EliminarManual(Guid id)
        {
            _logger.LogInformation("Eliminando manual: {ManualId}", id);

            var command = new EliminarManualCommand { ManualId = id };
            var resultado = await _mediator.Send(command);

            if (resultado.Exitoso)
            {
                _logger.LogInformation("Manual eliminado: {ManualId}", id);
                return Ok(resultado);
            }

            _logger.LogWarning("Error al eliminar manual {ManualId}: {Mensaje}", id, resultado.Mensaje);
            return BadRequest(resultado);
        }

        /// <summary>
        /// Actualizar el estado de un manual (Activo/Obsoleto/EnRevision)
        /// </summary>
        [HttpPatch("{id}/estado")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActualizarEstado(Guid id, [FromBody] ActualizarEstadoRequest request)
        {
            var command = new ActualizarEstadoManualCommand
            {
                ManualId = id,
                NuevoEstado = request.NuevoEstado
            };

            var resultado = await _mediator.Send(command);

            if (resultado.Exitoso)
                return Ok(resultado);

            return BadRequest(resultado);
        }

        /// <summary>
        /// Actualizar versión y descripción de un manual
        /// </summary>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ActualizarManual(Guid id, [FromBody] ActualizarManualRequest request)
        {
            var command = new ActualizarManualCommand
            {
                ManualId = id,
                Version = request.Version,
                Descripcion = request.Descripcion
            };

            var resultado = await _mediator.Send(command);

            if (resultado.Exitoso)
                return Ok(resultado);

            return BadRequest(resultado);
        }

        /// <summary>
        /// Obtener lista de categorías y subcategorías disponibles
        /// </summary>
        [HttpGet("categorias")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerCategorias()
        {
            var query = new ObtenerCategoriasQuery();
            var resultado = await _mediator.Send(query);
            return Ok(resultado);
        }
    }
}
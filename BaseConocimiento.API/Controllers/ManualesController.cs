using BaseConocimiento.API.DTOs.Manuales;
using BaseConocimiento.Application.UseCases.Manuales.Commands.ActualizarEstadoManual;
using BaseConocimiento.Application.UseCases.Manuales.Commands.ActualizarManual;
using BaseConocimiento.Application.UseCases.Manuales.Commands.EliminarManual;
using BaseConocimiento.Application.UseCases.Manuales.Commands.SubirManual;
using BaseConocimiento.Application.UseCases.Manuales.Queries.DescargarManual;
using BaseConocimiento.Application.UseCases.Manuales.Queries.ListarManuales;
using BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerCategorias;
using BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerManual;
using BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerManualPorId;
using BaseConocimiento.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BaseConocimiento.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        /// Listar manuales con filtros y paginación
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListarManuales(
            [FromQuery] Guid? categoriaId = null,
            [FromQuery] string? terminoBusqueda = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamañoPagina = 10)
        {
            var query = new ListarManualesQuery
            {
                CategoriaId = categoriaId,
                TerminoBusqueda = terminoBusqueda,
                Pagina = pagina,
                TamañoPagina = tamañoPagina
            };

            var response = await _mediator.Send(query);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Obtener detalle de un manual
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObtenerManual(Guid id)
        {
            var query = new ObtenerManualPorIdQuery { ManualId = id };
            var response = await _mediator.Send(query);

            if (!response.Exitoso)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarManualCommand command)
        {
            command.ManualId = id;
            var response = await _mediator.Send(command);
            return response.Exitoso ? Ok(response) : BadRequest(response);
        }

        [HttpPatch("{id:guid}/estado")]
        public async Task<IActionResult> ActualizarEstado(Guid id, [FromBody] ActualizarEstadoManualCommand command)
        {
            command.ManualId = id;
            var response = await _mediator.Send(command);
            return response.Exitoso ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            var response = await _mediator.Send(new EliminarManualCommand { ManualId = id });
            return response.Exitoso ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Subir nuevo manual
        /// </summary>
        [HttpPost]
        [RequestSizeLimit(52428800)] // 50MB
        public async Task<IActionResult> SubirManual([FromForm] SubirManualRequest request)
        {
            if (request.Archivo == null || request.Archivo.Length == 0)
                return BadRequest(new { mensaje = "No se recibió ningún archivo" });

            var command = new SubirManualCommand
            {
                Titulo = request.Titulo,
                CategoriaId = request.CategoriaId,
                Version = request.Version ?? "v1.0",
                Descripcion = request.Descripcion,
                NombreOriginal = request.Archivo.FileName,
                UsuarioId = request.UsuarioId,
                ArchivoStream = request.Archivo.OpenReadStream(),
                PesoArchivo = request.Archivo.Length
            };

            var response = await _mediator.Send(command);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Descargar manual
        /// </summary>
        [HttpGet("{id:guid}/descargar")]
        public async Task<IActionResult> Descargar(Guid id)
        {
            var response = await _mediator.Send(new DescargarManualQuery { ManualId = id });

            if (!response.Exitoso || response.ArchivoStream == null)
                return NotFound(response.Mensaje);

            return File(
                response.ArchivoStream,
                response.ContentType ?? "application/octet-stream",
                response.NombreArchivo
            );
        }

    }




}
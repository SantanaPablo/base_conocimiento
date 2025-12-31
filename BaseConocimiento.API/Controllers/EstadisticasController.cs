using BaseConocimiento.Application.UseCases.Consultas.Queries.ObtenerHistorialConsultas;
using BaseConocimiento.Application.UseCases.ObtenerEstadisticas;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BaseConocimiento.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadisticasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EstadisticasController> _logger;

        public EstadisticasController(IMediator mediator, ILogger<EstadisticasController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtener estadísticas generales del sistema
        /// </summary>
        [HttpGet("generales")]
        public async Task<IActionResult> ObtenerEstadisticasGenerales([FromQuery] DateTime? fechaInicio = null, [FromQuery] DateTime? fechaFin = null)
        {
            var query = new ObtenerEstadisticasQuery
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var response = await _mediator.Send(query);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Obtener historial de consultas con paginación
        /// </summary>
        [HttpGet("historial")]
        public async Task<IActionResult> ObtenerHistorialConsultas(
            [FromQuery] Guid? usuarioId = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamañoPagina = 20)
        {
            var query = new ObtenerHistorialConsultasQuery
            {
                UsuarioId = usuarioId,
                Pagina = pagina,
                TamañoPagina = tamañoPagina
            };

            var response = await _mediator.Send(query);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(response);
        }
    }
}

using BaseConocimiento.Application.UseCases.Consultas.Commands.ConsultarConConversacion;
using BaseConocimiento.Application.UseCases.Consultas.Queries.BuscarEnManuales;
using BaseConocimiento.Application.UseCases.Consultas.Queries.ConsultarBaseConocimiento;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BaseConocimiento.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ConsultasController> _logger;

        public ConsultasController(IMediator mediator, ILogger<ConsultasController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Consultar la base de conocimiento con conversación
        /// </summary>
        [HttpPost("consultar-con-conversacion")]
        public async Task<IActionResult> ConsultarConConversacion([FromBody] ConsultarConConversacionCommand command)
        {
            var response = await _mediator.Send(command);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Consulta simple sin historial
        /// </summary>
        [HttpPost("consultar")]
        public async Task<IActionResult> Consultar([FromBody] ConsultarBaseConocimientoQuery query)
        {
            var response = await _mediator.Send(query);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Buscar en manuales (solo resultados, sin respuesta generada)
        /// </summary>
        [HttpPost("buscar")]
        public async Task<IActionResult> BuscarEnManuales([FromBody] BuscarEnManualesQuery query)
        {
            var response = await _mediator.Send(query);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(response);
        }
    }

}
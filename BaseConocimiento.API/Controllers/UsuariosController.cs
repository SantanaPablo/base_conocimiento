using BaseConocimiento.Application.UseCases.Categorias.ActualizarCategoria;
using BaseConocimiento.Application.UseCases.Categorias.CrearCategoria;
using BaseConocimiento.Application.UseCases.Categorias.Queries.ObtenerCategoriaPorId;
using BaseConocimiento.Application.UseCases.Usuarios.Queries.ListarUsuarios;

using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BaseConocimiento.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(IMediator mediator, ILogger<UsuariosController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Listar todos los usuarios con filtros
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListarUsuarios([FromQuery] bool soloActivos = true, [FromQuery] string? departamento = null)
        {
            var query = new ListarUsuariosQuery
            {
                SoloActivos = soloActivos,
                Departamento = departamento
            };

            var response = await _mediator.Send(query);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Obtener detalle de un usuario
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObtenerUsuario(Guid id)
        {
            var query = new ObtenerUsuarioPorIdQuery { UsuarioId = id };
            var response = await _mediator.Send(query);

            if (!response.Exitoso)
                return NotFound(response);

            return Ok(response);
        }

        /// <summary>
        /// Crear nuevo usuario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioCommand command)
        {
            var response = await _mediator.Send(command);

            if (!response.Exitoso)
                return BadRequest(response);

            return CreatedAtAction(nameof(ObtenerUsuario), new { id = response.UsuarioId }, response);
        }

        /// <summary>
        /// Actualizar usuario existente
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> ActualizarUsuario(Guid id, [FromBody] ActualizarUsuarioCommand command)
        {
            command.UsuarioId = id;
            var response = await _mediator.Send(command);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(response);
        }
    }
}

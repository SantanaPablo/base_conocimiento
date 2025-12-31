using BaseConocimiento.Application.UseCases.Categorias.Commands.ActualizarCategoria;
using BaseConocimiento.Application.UseCases.Categorias.Commands.CrearCategoria;
using BaseConocimiento.Application.UseCases.Categorias.Queries.ListarCategorias;
using BaseConocimiento.Application.UseCases.Categorias.Queries.ObtenerCategoriaPorId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BaseConocimiento.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(IMediator mediator, ILogger<CategoriasController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las categorías con jerarquía
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListarCategorias([FromQuery] bool soloActivas = true, [FromQuery] bool incluirSubcategorias = true)
        {
            var query = new ListarCategoriasQuery
            {
                SoloActivas = soloActivas,
                IncluirSubcategorias = incluirSubcategorias
            };

            var response = await _mediator.Send(query);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Obtener detalle de una categoría
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObtenerCategoria(Guid id)
        {
            var query = new ObtenerCategoriaPorIdQuery { CategoriaId = id };
            var response = await _mediator.Send(query);

            if (!response.Exitoso)
                return NotFound(response);

            return Ok(response);
        }

        /// <summary>
        /// Crear nueva categoría
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaCommand command)
        {
            var response = await _mediator.Send(command);

            if (!response.Exitoso)
                return BadRequest(response);

            return CreatedAtAction(nameof(ObtenerCategoria), new { id = response.CategoriaId }, response);
        }

        /// <summary>
        /// Actualizar categoría existente
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> ActualizarCategoria(Guid id, [FromBody] ActualizarCategoriaCommand command)
        {
            command.CategoriaId = id;
            var response = await _mediator.Send(command);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(response);
        }
    }
}

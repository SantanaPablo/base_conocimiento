using Microsoft.AspNetCore.Mvc;
using MediatR;
using BaseConocimiento.Application.UseCases.Consultas.Queries.ConsultarBaseConocimiento;
using BaseConocimiento.Application.UseCases.Consultas.Queries.BuscarEnManuales;
using BaseConocimiento.API.DTOs.Consultas;

namespace BaseConocimiento.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
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
        /// Hacer una pregunta a la base de conocimiento usando RAG
        /// </summary>
        /// <param name="request">Pregunta y filtros opcionales</param>
        /// <returns>Respuesta generada por IA con fuentes citadas</returns>
        [HttpPost("preguntar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> HacerPregunta([FromBody] PreguntaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Pregunta))
                return BadRequest(new { Mensaje = "La pregunta no puede estar vacía" });

            _logger.LogInformation("Pregunta recibida: {Pregunta}", request.Pregunta);

            var query = new ConsultarBaseConocimientoQuery
            {
                Pregunta = request.Pregunta,
                Categoria = request.Categoria,
                TopK = request.TopK > 0 ? Math.Min(request.TopK, 10) : 5 // Máximo 10
            };

            var resultado = await _mediator.Send(query);

            if (resultado.Exitoso)
            {
                _logger.LogInformation("Respuesta generada para pregunta con {Count} fuentes",
                    resultado.Fuentes?.Count ?? 0);
                return Ok(resultado);
            }

            _logger.LogWarning("Error al generar respuesta: {Mensaje}", resultado.Mensaje);
            return BadRequest(resultado);
        }

        /// <summary>
        /// Buscar fragmentos similares en los manuales sin generar respuesta
        /// </summary>
        /// <param name="request">Texto de búsqueda y filtros</param>
        /// <returns>Lista de fragmentos relevantes ordenados por similitud</returns>
        [HttpPost("buscar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuscarEnManuales([FromBody] BuscarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TextoBusqueda))
                return BadRequest(new { Mensaje = "El texto de búsqueda no puede estar vacío" });

            _logger.LogInformation("Búsqueda: {Texto}", request.TextoBusqueda);

            var query = new BuscarEnManualesQuery
            {
                TextoBusqueda = request.TextoBusqueda,
                Categoria = request.Categoria,
                TopK = request.TopK > 0 ? Math.Min(request.TopK, 20) : 10 // Máximo 20
            };

            var resultado = await _mediator.Send(query);

            if (resultado.Exitoso)
            {
                _logger.LogInformation("Búsqueda completada: {Count} resultados",
                    resultado.Resultados?.Count ?? 0);
                return Ok(resultado);
            }

            return BadRequest(resultado);
        }

        /// <summary>
        /// Obtener estadísticas de uso de consultas
        /// </summary>
        [HttpGet("estadisticas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ObtenerEstadisticas()
        {
            // TODO: Implementar sistema de métricas
            return Ok(new
            {
                Mensaje = "Estadísticas no implementadas aún",
                TotalConsultas = 0,
                PromedioRespuesta = 0
            });
        }
    }
    
}
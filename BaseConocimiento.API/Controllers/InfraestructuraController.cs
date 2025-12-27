using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BaseConocimiento.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfraestructuraController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        public InfraestructuraController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpGet("estado")]
        public async Task<IActionResult> GetEstado()
        {
            var report = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                Status = report.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                Servicios = report.Entries.Select(e => new
                {
                    Nombre = e.Key,
                    Estado = e.Value.Status.ToString(),
                    Descripcion = e.Value.Description,
                    Duracion = e.Value.Duration.TotalMilliseconds + "ms"
                })
            };

            return report.Status == HealthStatus.Healthy ? Ok(response) : StatusCode(503, response);
        }
    }
}
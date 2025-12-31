using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.ObtenerEstadisticas
{
    public class ObtenerEstadisticasHandler : IRequestHandler<ObtenerEstadisticasQuery, ObtenerEstadisticasResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObtenerEstadisticasHandler> _logger;

        public ObtenerEstadisticasHandler(IUnitOfWork unitOfWork, ILogger<ObtenerEstadisticasHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ObtenerEstadisticasResponse> Handle(ObtenerEstadisticasQuery request, CancellationToken ct)
        {
            try
            {
                var fechaInicio = request.FechaInicio ?? DateTime.UtcNow.AddMonths(-1);
                var fechaFin = request.FechaFin ?? DateTime.UtcNow;

                var totalManuales = await _unitOfWork.Manuales.ContarAsync(ct);
                var totalConsultas = await _unitOfWork.Consultas.ContarAsync(ct);
                var totalUsuarios = await _unitOfWork.Usuarios.ContarActivosAsync(ct);
                var manualesActivos = await _unitOfWork.Manuales.ContarActivosAsync(ct);
                var consultasHoy = await _unitOfWork.Consultas.ContarPorFechaAsync(DateTime.UtcNow.Date, ct);

               
                var tiempoPromedio = await _unitOfWork.Consultas.ObtenerTiempoPromedioRespuestaAsync(ct);

               
                var manualesPorCategoria = await _unitOfWork.Categorias.ObtenerEstadisticasManualesAsync(ct);

                var topManuales = await _unitOfWork.Manuales.ObtenerMasConsultadosAsync(10, ct);
                var topManualesDto = topManuales.Select(m => new ManualMasConsultadoDto
                {
                    ManualId = m.Id,
                    Titulo = m.Titulo,
                    Categoria = m.Categoria?.Nombre ?? "Sin categoría",
                    NumeroConsultas = m.NumeroConsultas,
                    UltimaConsulta = m.UltimaConsulta
                }).ToList();

                var usuariosActivos = await _unitOfWork.Usuarios.ObtenerMasActivosAsync(10, ct);
                var usuariosActivosDto = usuariosActivos.Select(u => new UsuarioActivoDto
                {
                    UsuarioId = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    TotalConsultas = u.ConsultasRealizadas?.Count ?? 0,
                    UltimaConsulta = u.ConsultasRealizadas?.OrderByDescending(c => c.FechaConsulta).FirstOrDefault()?.FechaConsulta
                }).ToList();

                var estadisticas = new EstadisticasGeneralesDto
                {
                    TotalManuales = totalManuales,
                    TotalConsultas = totalConsultas,
                    TotalUsuarios = totalUsuarios,
                    TiempoPromedioRespuesta = Math.Round(tiempoPromedio, 2),
                    ManualesActivos = manualesActivos,
                    ConsultasHoy = consultasHoy,
                    ManualesPorCategoria = manualesPorCategoria.Select(x => new CategoriaEstadisticaDto
                    {
                        Categoria = x.Categoria,
                        TotalManuales = x.TotalManuales,
                        TotalConsultas = x.TotalConsultas
                    }).ToList(),
                    TopManuales = topManualesDto,
                    UsuariosMasActivos = usuariosActivosDto
                };

                return new ObtenerEstadisticasResponse
                {
                    Exitoso = true,
                    Estadisticas = estadisticas
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return new ObtenerEstadisticasResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error: {ex.Message}"
                };
            }
        }
    }
}

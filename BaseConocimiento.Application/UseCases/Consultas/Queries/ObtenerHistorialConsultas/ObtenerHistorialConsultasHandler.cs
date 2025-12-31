using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Consultas.Queries.ObtenerHistorialConsultas
{
    public class ObtenerHistorialConsultasHandler : IRequestHandler<ObtenerHistorialConsultasQuery, ObtenerHistorialConsultasResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObtenerHistorialConsultasHandler> _logger;

        public ObtenerHistorialConsultasHandler(IUnitOfWork unitOfWork, ILogger<ObtenerHistorialConsultasHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ObtenerHistorialConsultasResponse> Handle(ObtenerHistorialConsultasQuery request, CancellationToken ct)
        {
            try
            {
                var (consultas, total) = await _unitOfWork.Consultas.ListarPaginadoAsync(
                    usuarioId: request.UsuarioId,
                    pagina: request.Pagina,
                    tamañoPagina: request.TamañoPagina,
                    ct
                );

                var consultasDto = consultas.Select(c => new ConsultaHistorialDto
                {
                    Id = c.Id,
                    Pregunta = c.Pregunta,
                    RespuestaResumen = c.Respuesta.Length > 200 ? c.Respuesta.Substring(0, 200) + "..." : c.Respuesta,
                    Usuario = c.Usuario?.NombreCompleto ?? "Usuario desconocido",
                    FechaConsulta = c.FechaConsulta,
                    TiempoRespuestaMs = c.TiempoRespuestaMs,
                    CantidadFuentes = c.ManualesConsultados?.Count ?? 0,
                    FueUtil = c.FueUtil
                }).ToList();

                var totalPaginas = (int)Math.Ceiling(total / (double)request.TamañoPagina);

                return new ObtenerHistorialConsultasResponse
                {
                    Exitoso = true,
                    Consultas = consultasDto,
                    TotalRegistros = total,
                    PaginaActual = request.Pagina,
                    TotalPaginas = totalPaginas
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de consultas");
                return new ObtenerHistorialConsultasResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error: {ex.Message}"
                };
            }
        }
    }
}

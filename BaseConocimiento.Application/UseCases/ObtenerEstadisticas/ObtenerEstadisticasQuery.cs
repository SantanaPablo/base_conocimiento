using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.ObtenerEstadisticas
{
    public class ObtenerEstadisticasQuery : IRequest<ObtenerEstadisticasResponse>
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    public class ObtenerEstadisticasResponse
    {
        public bool Exitoso { get; set; }
        public EstadisticasGeneralesDto Estadisticas { get; set; }
        public string Mensaje { get; set; }
    }

    public class EstadisticasGeneralesDto
    {
        public int TotalManuales { get; set; }
        public int TotalConsultas { get; set; }
        public int TotalUsuarios { get; set; }
        public double TiempoPromedioRespuesta { get; set; }
        public int ManualesActivos { get; set; }
        public int ConsultasHoy { get; set; }
        public List<CategoriaEstadisticaDto> ManualesPorCategoria { get; set; } = new();
        public List<ManualMasConsultadoDto> TopManuales { get; set; } = new();
        public List<UsuarioActivoDto> UsuariosMasActivos { get; set; } = new();
    }

    public class CategoriaEstadisticaDto
    {
        public string Categoria { get; set; }
        public int TotalManuales { get; set; }
        public int TotalConsultas { get; set; }
    }

    public class ManualMasConsultadoDto
    {
        public Guid ManualId { get; set; }
        public string Titulo { get; set; }
        public string Categoria { get; set; }
        public int NumeroConsultas { get; set; }
        public DateTime? UltimaConsulta { get; set; }
    }

    public class UsuarioActivoDto
    {
        public Guid UsuarioId { get; set; }
        public string NombreCompleto { get; set; }
        public int TotalConsultas { get; set; }
        public DateTime? UltimaConsulta { get; set; }
    }
}

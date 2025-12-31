using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Consultas.Queries.ObtenerHistorialConsultas
{
    public class ObtenerHistorialConsultasQuery : IRequest<ObtenerHistorialConsultasResponse>
    {
        public Guid? UsuarioId { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 20;
    }

    public class ObtenerHistorialConsultasResponse
    {
        public bool Exitoso { get; set; }
        public List<ConsultaHistorialDto> Consultas { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public string Mensaje { get; set; }
    }

    public class ConsultaHistorialDto
    {
        public Guid Id { get; set; }
        public string Pregunta { get; set; }
        public string RespuestaResumen { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaConsulta { get; set; }
        public long TiempoRespuestaMs { get; set; }
        public int CantidadFuentes { get; set; }
        public bool FueUtil { get; set; }
    }
}

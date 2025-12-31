using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Queries.ListarManuales
{
    public class ListarManualesQuery : IRequest<ListarManualesResponse>
    {
        public Guid? CategoriaId { get; set; }
        public string? TerminoBusqueda { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 10;
        public OrdenManuales OrdenarPor { get; set; } = OrdenManuales.FechaDesc;
    }

    public enum OrdenManuales
    {
        FechaDesc,
        FechaAsc,
        TituloAsc,
        TituloDesc,
        MasConsultado
    }

    public class ListarManualesResponse
    {
        public bool Exitoso { get; set; }
        public List<ManualListadoDto> Manuales { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public string Mensaje { get; set; }
    }

    public class ManualListadoDto
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Categoria { get; set; }
        public string? SubCategoria { get; set; }
        public string Version { get; set; }
        public DateTime FechaSubida { get; set; }
        public string SubidoPor { get; set; }
        public long TamañoBytes { get; set; }
        public string TamañoFormateado { get; set; }
        public int NumeroConsultas { get; set; }
        public DateTime? UltimaConsulta { get; set; }
        public string Estado { get; set; }
    }
}

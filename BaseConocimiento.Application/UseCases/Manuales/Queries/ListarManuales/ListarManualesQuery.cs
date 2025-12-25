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
        public string Categoria { get; set; }
        public string SubCategoria { get; set; }
        public EstadoManual? Estado { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 20;
    }

    public class ListarManualesResponse
    {
        public List<ManualDto> Manuales { get; set; }
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TamañoPagina { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class ManualDto
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Categoria { get; set; }
        public string SubCategoria { get; set; }
        public string Version { get; set; }
        public string Descripcion { get; set; }
        public string NombreOriginal { get; set; }
        public DateTime FechaSubida { get; set; }
        public double PesoArchivoMB { get; set; }
        public EstadoManual Estado { get; set; }
        public string UsuarioId { get; set; }
    }
}

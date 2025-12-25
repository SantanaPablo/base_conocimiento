using BaseConocimiento.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerManual
{
    public class ObtenerManualQuery : IRequest<ObtenerManualResponse>
    {
        public Guid ManualId { get; set; }
    }

    public class ObtenerManualResponse
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Categoria { get; set; }
        public string SubCategoria { get; set; }
        public string Version { get; set; }
        public string Descripcion { get; set; }
        public string NombreOriginal { get; set; }
        public string RutaLocal { get; set; }
        public DateTime FechaSubida { get; set; }
        public double PesoArchivoMB { get; set; }
        public EstadoManual Estado { get; set; }
        public string UsuarioId { get; set; }
    }
}

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Queries.DescargarManual
{
    public class DescargarManualQuery : IRequest<DescargarManualResponse>
    {
        public Guid ManualId { get; set; }
    }

    public class DescargarManualResponse
    {
        public bool Exitoso { get; set; }
        public string? Mensaje { get; set; }

        public Stream? ArchivoStream { get; set; }
        public string? NombreArchivo { get; set; }
        public string? ContentType { get; set; }
    }
}

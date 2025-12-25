using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.ActualizarManual
{
    public class ActualizarManualCommand : IRequest<ActualizarManualResponse>
    {
        public Guid ManualId { get; set; }
        public string Version { get; set; }
        public string Descripcion { get; set; }
    }

    public class ActualizarManualResponse
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
    }
}

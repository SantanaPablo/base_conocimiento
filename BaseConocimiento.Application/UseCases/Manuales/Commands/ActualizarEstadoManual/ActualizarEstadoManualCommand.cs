using BaseConocimiento.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.ActualizarEstadoManual
{
    public class ActualizarEstadoManualCommand : IRequest<ActualizarEstadoManualResponse>
    {
        public Guid ManualId { get; set; }
        public EstadoManual NuevoEstado { get; set; }
    }

    public class ActualizarEstadoManualResponse
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
    }
}

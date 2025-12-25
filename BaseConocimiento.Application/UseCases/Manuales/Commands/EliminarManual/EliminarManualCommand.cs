using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.EliminarManual
{
    public class EliminarManualCommand : IRequest<EliminarManualResponse>
    {
        public Guid ManualId { get; set; }
    }

    public class EliminarManualResponse
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
    }
}

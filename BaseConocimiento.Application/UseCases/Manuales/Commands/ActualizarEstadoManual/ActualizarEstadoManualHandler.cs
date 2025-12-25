using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.ActualizarEstadoManual
{
    public class ActualizarEstadoManualHandler : IRequestHandler<ActualizarEstadoManualCommand, ActualizarEstadoManualResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActualizarEstadoManualHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ActualizarEstadoManualResponse> Handle(
            ActualizarEstadoManualCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(request.ManualId, cancellationToken);

                if (manual == null)
                {
                    return new ActualizarEstadoManualResponse
                    {
                        Exitoso = false,
                        Mensaje = "Manual no encontrado"
                    };
                }

                manual.ActualizarEstado(request.NuevoEstado);

                await _unitOfWork.Manuales.ActualizarAsync(manual, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new ActualizarEstadoManualResponse
                {
                    Exitoso = true,
                    Mensaje = $"Estado actualizado a {request.NuevoEstado}"
                };
            }
            catch (Exception ex)
            {
                return new ActualizarEstadoManualResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error al actualizar estado: {ex.Message}"
                };
            }
        }
    }
}

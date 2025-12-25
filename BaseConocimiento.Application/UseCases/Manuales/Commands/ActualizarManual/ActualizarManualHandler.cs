using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.ActualizarManual
{
    public class ActualizarManualHandler : IRequestHandler<ActualizarManualCommand, ActualizarManualResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActualizarManualHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ActualizarManualResponse> Handle(
            ActualizarManualCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(request.ManualId, cancellationToken);

                if (manual == null)
                {
                    return new ActualizarManualResponse
                    {
                        Exitoso = false,
                        Mensaje = "Manual no encontrado"
                    };
                }

                if (!string.IsNullOrWhiteSpace(request.Version))
                    manual.ActualizarVersion(request.Version);

                if (!string.IsNullOrWhiteSpace(request.Descripcion))
                    manual.ActualizarDescripcion(request.Descripcion);

                await _unitOfWork.Manuales.ActualizarAsync(manual, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new ActualizarManualResponse
                {
                    Exitoso = true,
                    Mensaje = "Manual actualizado exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new ActualizarManualResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error al actualizar manual: {ex.Message}"
                };
            }
        }
    }
}

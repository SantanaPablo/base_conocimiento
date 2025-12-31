using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.ActualizarManual
{
    public class ActualizarManualHandler : IRequestHandler<ActualizarManualCommand, ActualizarManualResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActualizarManualHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ActualizarManualResponse> Handle(ActualizarManualCommand request, CancellationToken ct)
        {
            try
            {
                var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(request.ManualId, ct);

                if (manual == null)
                {
                    return new ActualizarManualResponse { Exitoso = false, Mensaje = "Manual no encontrado." };
                }

                // Actualizar campos permitidos
                if (!string.IsNullOrWhiteSpace(request.Version))
                    manual.ActualizarVersion(request.Version);

                if (!string.IsNullOrWhiteSpace(request.Descripcion))
                    manual.ActualizarDescripcion(request.Descripcion);

                await _unitOfWork.Manuales.ActualizarAsync(manual, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                return new ActualizarManualResponse { Exitoso = true, Mensaje = "Manual actualizado correctamente." };
            }
            catch (Exception ex)
            {
                return new ActualizarManualResponse { Exitoso = false, Mensaje = $"Error: {ex.Message}" };
            }
        }
    }
}
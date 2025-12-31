using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.ActualizarEstadoManual
{
    public class ActualizarEstadoManualHandler : IRequestHandler<ActualizarEstadoManualCommand, ActualizarEstadoManualResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActualizarEstadoManualHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ActualizarEstadoManualResponse> Handle(ActualizarEstadoManualCommand request, CancellationToken ct)
        {
            try
            {
                var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(request.ManualId, ct);

                if (manual == null)
                    return new ActualizarEstadoManualResponse { Exitoso = false, Mensaje = "Manual no encontrado." };

                manual.ActualizarEstado(request.NuevoEstado);

                await _unitOfWork.Manuales.ActualizarAsync(manual, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                return new ActualizarEstadoManualResponse { Exitoso = true, Mensaje = "Estado actualizado." };
            }
            catch (Exception ex)
            {
                return new ActualizarEstadoManualResponse { Exitoso = false, Mensaje = ex.Message };
            }
        }
    }
}
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.Storage;
using BaseConocimiento.Application.Interfaces.VectorStore;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.EliminarManual
{
    public class EliminarManualHandler : IRequestHandler<EliminarManualCommand, EliminarManualResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;
        private readonly IQdrantService _qdrantService;
        private readonly ILogger<EliminarManualHandler> _logger;

        public EliminarManualHandler(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage,
            IQdrantService qdrantService,
            ILogger<EliminarManualHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _qdrantService = qdrantService;
            _logger = logger;
        }

        public async Task<EliminarManualResponse> Handle(EliminarManualCommand request, CancellationToken ct)
        {
            try
            {
                var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(request.ManualId, ct);

                if (manual == null)
                {
                    return new EliminarManualResponse { Exitoso = false, Mensaje = "Manual no encontrado." };
                }

                //Borrar de SQL en cascada
                await _unitOfWork.Manuales.EliminarAsync(manual, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                //Borrar fisico
                await _fileStorage.EliminarArchivoAsync(manual.Id);

                //Borrar de Qdrant
                await _qdrantService.EliminarVectoresAsync(manual.Id);

                _logger.LogInformation("Manual {Id} purgado del sistema", manual.Id);

                return new EliminarManualResponse { Exitoso = true, Mensaje = "Manual eliminado de todos los repositorios." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo al purgar manual {Id}", request.ManualId);
                return new EliminarManualResponse { Exitoso = false, Mensaje = ex.Message };
            }
        }
    }
}
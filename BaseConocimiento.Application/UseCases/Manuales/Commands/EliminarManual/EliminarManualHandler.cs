using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.Storage;
using BaseConocimiento.Application.Interfaces.VectorStore;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.EliminarManual
{
    public class EliminarManualHandler : IRequestHandler<EliminarManualCommand, EliminarManualResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;
        private readonly IQdrantService _qdrantService;

        public EliminarManualHandler(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage,
            IQdrantService qdrantService)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _qdrantService = qdrantService;
        }

        public async Task<EliminarManualResponse> Handle(
            EliminarManualCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(request.ManualId, cancellationToken);

                if (manual == null)
                {
                    return new EliminarManualResponse
                    {
                        Exitoso = false,
                        Mensaje = "Manual no encontrado"
                    };
                }

                await _unitOfWork.Manuales.EliminarAsync(manual, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _fileStorage.EliminarArchivoAsync(manual.Id);
                await _qdrantService.EliminarVectoresAsync(manual.Id);

                return new EliminarManualResponse
                {
                    Exitoso = true,
                    Mensaje = "Manual eliminado exitosamente de los 3 sistemas"
                };
            }
            catch (Exception ex)
            {
                return new EliminarManualResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error al eliminar manual: {ex.Message}"
                };
            }
        }
    }
}

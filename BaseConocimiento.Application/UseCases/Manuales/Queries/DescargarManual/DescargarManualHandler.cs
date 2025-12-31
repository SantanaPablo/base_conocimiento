using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.Storage;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Queries.DescargarManual
{
    public class DescargarManualHandler
     : IRequestHandler<DescargarManualQuery, DescargarManualResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<DescargarManualHandler> _logger;

        public DescargarManualHandler(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage,
            ILogger<DescargarManualHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _logger = logger;
        }

        public async Task<DescargarManualResponse> Handle(
            DescargarManualQuery request,
            CancellationToken ct)
        {
            try
            {
                var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(request.ManualId, ct);

                if (manual == null)
                {
                    return new DescargarManualResponse
                    {
                        Exitoso = false,
                        Mensaje = "Manual no encontrado"
                    };
                }

                var archivoStream = await _fileStorage.ObtenerArchivoAsync(manual.Id);

                return new DescargarManualResponse
                {
                    Exitoso = true,
                    ArchivoStream = archivoStream,
                    NombreArchivo = manual.NombreOriginal,
                    ContentType = "application/octet-stream"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar manual {ManualId}", request.ManualId);
                return new DescargarManualResponse
                {
                    Exitoso = false,
                    Mensaje = "Error al descargar archivo"
                };
            }
        }
    }
}
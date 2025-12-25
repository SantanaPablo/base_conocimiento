using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerManual
{
    public class ObtenerManualHandler : IRequestHandler<ObtenerManualQuery, ObtenerManualResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtenerManualHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ObtenerManualResponse> Handle(
            ObtenerManualQuery request,
            CancellationToken cancellationToken)
        {
            var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(request.ManualId, cancellationToken);

            if (manual == null)
                return null;

            return new ObtenerManualResponse
            {
                Id = manual.Id,
                Titulo = manual.Titulo,
                Categoria = manual.Categoria,
                SubCategoria = manual.SubCategoria,
                Version = manual.Version,
                Descripcion = manual.Descripcion,
                NombreOriginal = manual.NombreOriginal,
                RutaLocal = manual.RutaLocal,
                FechaSubida = manual.FechaSubida,
                PesoArchivoMB = Math.Round(manual.PesoArchivo / 1024.0 / 1024.0, 2),
                Estado = manual.Estado,
                UsuarioId = manual.UsuarioId
            };
        }
    }
}

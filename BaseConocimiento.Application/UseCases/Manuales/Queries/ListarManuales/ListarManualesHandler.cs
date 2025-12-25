using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Queries.ListarManuales
{
    public class ListarManualesHandler : IRequestHandler<ListarManualesQuery, ListarManualesResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListarManualesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ListarManualesResponse> Handle(
            ListarManualesQuery request,
            CancellationToken cancellationToken)
        {
            var (manuales, total) = await _unitOfWork.Manuales.ObtenerPaginadoAsync(
                request.Pagina,
                request.TamañoPagina,
                request.Categoria,
                request.SubCategoria,
                request.Estado,
                cancellationToken
            );

            var manualesDto = manuales.Select(m => new ManualDto
            {
                Id = m.Id,
                Titulo = m.Titulo,
                Categoria = m.Categoria,
                SubCategoria = m.SubCategoria,
                Version = m.Version,
                Descripcion = m.Descripcion,
                NombreOriginal = m.NombreOriginal,
                FechaSubida = m.FechaSubida,
                PesoArchivoMB = Math.Round(m.PesoArchivo / 1024.0 / 1024.0, 2),
                Estado = m.Estado,
                UsuarioId = m.UsuarioId
            }).ToList();

            return new ListarManualesResponse
            {
                Manuales = manualesDto,
                Total = total,
                Pagina = request.Pagina,
                TamañoPagina = request.TamañoPagina,
                TotalPaginas = (int)Math.Ceiling(total / (double)request.TamañoPagina)
            };
        }
    }
}

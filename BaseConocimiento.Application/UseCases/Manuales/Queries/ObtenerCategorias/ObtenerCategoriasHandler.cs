using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerCategorias
{
    public class ObtenerCategoriasHandler : IRequestHandler<ObtenerCategoriasQuery, ObtenerCategoriasResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtenerCategoriasHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ObtenerCategoriasResponse> Handle(
            ObtenerCategoriasQuery request,
            CancellationToken cancellationToken)
        {
            var categoriasDict = await _unitOfWork.Manuales.ObtenerCategoriasYSubcategoriasAsync(cancellationToken);

            var categorias = categoriasDict.Select(kvp => new CategoriaDto
            {
                Categoria = kvp.Key,
                SubCategorias = kvp.Value
            }).ToList();

            return new ObtenerCategoriasResponse
            {
                Categorias = categorias
            };
        }
    }
}

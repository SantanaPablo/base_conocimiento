using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;

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
            CancellationToken ct)
        {

            var categoriasEntity = await _unitOfWork.Categorias.ObtenerTodasAsync(ct);

            var categorias = categoriasEntity.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Color = c.Color,
                Icono = c.Icono,
                
                SubCategorias = c.SubCategorias?.Select(s => s.Nombre).ToList() ?? new List<string>(),
                CantidadManuales = c.Manuales?.Count ?? 0
            }).ToList();

            return new ObtenerCategoriasResponse
            {
                Categorias = categorias
            };
        }
    }
}
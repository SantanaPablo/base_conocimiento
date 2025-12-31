using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.Queries.ObtenerCategoriaPorId
{
    public class ObtenerCategoriaPorIdHandler : IRequestHandler<ObtenerCategoriaPorIdQuery, ObtenerCategoriaPorIdResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtenerCategoriaPorIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ObtenerCategoriaPorIdResponse> Handle(ObtenerCategoriaPorIdQuery request, CancellationToken ct)
        {
            var categoria = await _unitOfWork.Categorias.ObtenerConDetallesAsync(request.CategoriaId, ct: ct);

            if (categoria == null)
            {
                return new ObtenerCategoriaPorIdResponse
                {
                    Exitoso = false,
                    Mensaje = "Categoría no encontrada"
                };
            }

            return new ObtenerCategoriaPorIdResponse
            {
                Exitoso = true,
                Categoria = new CategoriaDetalleDto
                {
                    Id = categoria.Id,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,
                    Color = categoria.Color,
                    Icono = categoria.Icono,
                    TotalManuales = categoria.Manuales?.Count ?? 0,
                    TotalSubcategorias = categoria.SubCategorias?.Count ?? 0,
                    FechaCreacion = categoria.FechaCreacion
                }
            };
        }
    }
}

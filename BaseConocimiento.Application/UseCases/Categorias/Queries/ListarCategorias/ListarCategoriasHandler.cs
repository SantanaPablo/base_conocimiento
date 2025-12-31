using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.Queries.ListarCategorias
{
    public class ListarCategoriasHandler : IRequestHandler<ListarCategoriasQuery, ListarCategoriasResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListarCategoriasHandler> _logger;

        public ListarCategoriasHandler(IUnitOfWork unitOfWork, ILogger<ListarCategoriasHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ListarCategoriasResponse> Handle(ListarCategoriasQuery request, CancellationToken ct)
        {
            try
            {
                var categorias = await _unitOfWork.Categorias.ListarConDetallesAsync(
                    soloActivas: request.SoloActivas,
                    incluirSubcategorias: request.IncluirSubcategorias,
                    ct
                );

                var categoriasDto = categorias
                    .Where(c => c.CategoriaPadreId == null) // Solo categorías raíz
                    .Select(c => MapearCategoriaDto(c, request.IncluirSubcategorias))
                    .OrderBy(c => c.Orden)
                    .ToList();

                return new ListarCategoriasResponse
                {
                    Exitoso = true,
                    Categorias = categoriasDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar categorías");
                return new ListarCategoriasResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error: {ex.Message}"
                };
            }
        }

        private CategoriaDto MapearCategoriaDto(Domain.Entities.Categoria categoria, bool incluirSubs)
        {
            var dto = new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                CategoriaPadreId = categoria.CategoriaPadreId,
                Color = categoria.Color,
                Icono = categoria.Icono,
                Orden = categoria.Orden,
                EsActiva = categoria.EsActiva,
                TotalManuales = categoria.Manuales?.Count(m => m.EstaActivo()) ?? 0
            };

            if (incluirSubs && categoria.SubCategorias?.Any() == true)
            {
                dto.SubCategorias = categoria.SubCategorias
                    .Select(sub => MapearCategoriaDto(sub, false))
                    .OrderBy(s => s.Orden)
                    .ToList();
            }

            return dto;
        }
    }
}

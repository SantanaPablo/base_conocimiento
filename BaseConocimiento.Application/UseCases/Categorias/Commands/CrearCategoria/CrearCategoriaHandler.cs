using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.Commands.CrearCategoria
{
    public class CrearCategoriaHandler : IRequestHandler<CrearCategoriaCommand, CrearCategoriaResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CrearCategoriaHandler> _logger;

        public CrearCategoriaHandler(IUnitOfWork unitOfWork, ILogger<CrearCategoriaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CrearCategoriaResponse> Handle(CrearCategoriaCommand request, CancellationToken ct)
        {
            try
            {
                // Validar que no exista otra categoría con el mismo nombre
                var existente = await _unitOfWork.Categorias.ObtenerPorNombreAsync(request.Nombre, ct);
                if (existente != null)
                {
                    return new CrearCategoriaResponse
                    {
                        Exitoso = false,
                        Mensaje = "Ya existe una categoría con ese nombre"
                    };
                }

                // Si tiene padre, validar que exista
                if (request.CategoriaPadreId.HasValue)
                {
                    var padre = await _unitOfWork.Categorias.ObtenerPorIdAsync(request.CategoriaPadreId.Value, ct);
                    if (padre == null)
                    {
                        return new CrearCategoriaResponse
                        {
                            Exitoso = false,
                            Mensaje = "La categoría padre no existe"
                        };
                    }
                }

                var categoria = Categoria.Crear(request.Nombre, request.Descripcion, request.CategoriaPadreId);

                if (!string.IsNullOrEmpty(request.Color))
                    categoria.AsignarColor(request.Color);

                if (!string.IsNullOrEmpty(request.Icono))
                    categoria.AsignarIcono(request.Icono);

                await _unitOfWork.Categorias.AgregarAsync(categoria, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                _logger.LogInformation("Categoría creada: {CategoriaId} - {Nombre}", categoria.Id, categoria.Nombre);

                return new CrearCategoriaResponse
                {
                    Exitoso = true,
                    CategoriaId = categoria.Id,
                    Mensaje = "Categoría creada exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría: {Nombre}", request.Nombre);
                return new CrearCategoriaResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error: {ex.Message}"
                };
            }
        }
    }
}

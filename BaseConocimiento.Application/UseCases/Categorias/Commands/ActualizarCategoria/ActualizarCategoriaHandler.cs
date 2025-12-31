using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.Commands.ActualizarCategoria
{
    public class ActualizarCategoriaHandler : IRequestHandler<ActualizarCategoriaCommand, ActualizarCategoriaResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ActualizarCategoriaHandler> _logger;

        public ActualizarCategoriaHandler(IUnitOfWork unitOfWork, ILogger<ActualizarCategoriaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ActualizarCategoriaResponse> Handle(ActualizarCategoriaCommand request, CancellationToken ct)
        {
            try
            {
                var categoria = await _unitOfWork.Categorias.ObtenerPorIdAsync(request.CategoriaId, ct);
                if (categoria == null)
                {
                    return new ActualizarCategoriaResponse
                    {
                        Exitoso = false,
                        Mensaje = "Categoría no encontrada"
                    };
                }

                if (!string.IsNullOrEmpty(request.Nombre))
                    categoria.ActualizarNombre(request.Nombre);

                if (request.Color != null)
                    categoria.AsignarColor(request.Color);

                if (request.Icono != null)
                    categoria.AsignarIcono(request.Icono);

                if (request.Orden.HasValue)
                    categoria.CambiarOrden(request.Orden.Value);

                await _unitOfWork.SaveChangesAsync(ct);

                return new ActualizarCategoriaResponse
                {
                    Exitoso = true,
                    Mensaje = "Categoría actualizada exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría: {CategoriaId}", request.CategoriaId);
                return new ActualizarCategoriaResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error: {ex.Message}"
                };
            }
        }
    }
}

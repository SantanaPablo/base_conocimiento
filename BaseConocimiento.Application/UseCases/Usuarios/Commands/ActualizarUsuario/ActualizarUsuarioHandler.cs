using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.ActualizarCategoria
{
    public class ActualizarUsuarioHandler : IRequestHandler<ActualizarUsuarioCommand, ActualizarUsuarioResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ActualizarUsuarioHandler> _logger;

        public ActualizarUsuarioHandler(IUnitOfWork unitOfWork, ILogger<ActualizarUsuarioHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ActualizarUsuarioResponse> Handle(ActualizarUsuarioCommand request, CancellationToken ct)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(request.UsuarioId, ct);
                if (usuario == null)
                {
                    return new ActualizarUsuarioResponse
                    {
                        Exitoso = false,
                        Mensaje = "Usuario no encontrado"
                    };
                }

                if (request.Departamento != null)
                    usuario.AsignarDepartamento(request.Departamento);

                if (request.Rol.HasValue)
                    usuario.CambiarRol(request.Rol.Value);

                if (request.EsActivo.HasValue)
                {
                    if (request.EsActivo.Value)
                        usuario.Activar();
                    else
                        usuario.Desactivar();
                }

                await _unitOfWork.SaveChangesAsync(ct);

                return new ActualizarUsuarioResponse
                {
                    Exitoso = true,
                    Mensaje = "Usuario actualizado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {UsuarioId}", request.UsuarioId);
                return new ActualizarUsuarioResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error: {ex.Message}"
                };
            }
        }
    }
}

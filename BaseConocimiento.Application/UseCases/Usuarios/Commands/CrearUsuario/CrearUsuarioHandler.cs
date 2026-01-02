using BaseConocimiento.Application.Interfaces.Auth;
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.CrearCategoria
{
    public class CrearUsuarioHandler : IRequestHandler<CrearUsuarioCommand, CrearUsuarioResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CrearUsuarioHandler> _logger;
        private readonly IPasswordHasher _passwordHasher;

        public CrearUsuarioHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ILogger<CrearUsuarioHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<CrearUsuarioResponse> Handle(CrearUsuarioCommand request, CancellationToken ct)
        {
            try
            {
                var existente = await _unitOfWork.Usuarios.ObtenerPorEmailAsync(request.Email, ct);
                if (existente != null)
                {
                    return new CrearUsuarioResponse { Exitoso = false, Mensaje = "Ya existe un usuario con ese email" };
                }

                var usuario = Usuario.Crear(request.NombreCompleto, request.Email, request.Rol);

                var passwordHash = _passwordHasher.HashPassword(request.Password);
                usuario.AsignarPassword(passwordHash);

                if (!string.IsNullOrEmpty(request.Departamento))
                    usuario.AsignarDepartamento(request.Departamento);

                await _unitOfWork.Usuarios.AgregarAsync(usuario, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                _logger.LogInformation("Usuario creado: {UsuarioId}", usuario.Id);

                return new CrearUsuarioResponse { Exitoso = true, UsuarioId = usuario.Id, Mensaje = "Usuario creado exitosamente" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {Email}", request.Email);
                return new CrearUsuarioResponse { Exitoso = false, Mensaje = $"Error: {ex.Message}" };
            }
        }
    }
}


using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using BaseConocimiento.Application.Interfaces.Auth;

namespace BaseConocimiento.Application.UseCases.Auth.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ILogger<LoginCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken ct)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return new LoginCommandResponse
                    {
                        Exitoso = false,
                        Mensaje = "Email y contraseña son requeridos"
                    };
                }

                // Buscar usuario por email
                var usuario = await _unitOfWork.Usuarios.ObtenerPorEmailAsync(request.Email.ToLowerInvariant(), ct);

                if (usuario == null)
                {
                    _logger.LogWarning("Intento de login con email no registrado: {Email}", request.Email);
                    return new LoginCommandResponse
                    {
                        Exitoso = false,
                        Mensaje = "Credenciales inválidas"
                    };
                }

                // Verificar que el usuario esté activo
                if (!usuario.EsActivo)
                {
                    _logger.LogWarning("Intento de login con usuario inactivo: {Email}", request.Email);
                    return new LoginCommandResponse
                    {
                        Exitoso = false,
                        Mensaje = "Usuario inactivo. Contacte al administrador."
                    };
                }

                // Verificar password
                if (!_passwordHasher.VerifyPassword(request.Password, usuario.PasswordHash))
                {
                    _logger.LogWarning("Intento de login con contraseña incorrecta: {Email}", request.Email);
                    return new LoginCommandResponse
                    {
                        Exitoso = false,
                        Mensaje = "Credenciales inválidas"
                    };
                }

                // Actualizar último acceso
                usuario.ActualizarAcceso();
                await _unitOfWork.SaveChangesAsync(ct);

                _logger.LogInformation("Login exitoso: {Email} - {UsuarioId}", usuario.Email, usuario.Id);

                return new LoginCommandResponse
                {
                    Exitoso = true,
                    UsuarioId = usuario.Id,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email,
                    Rol = usuario.Rol.ToString(),
                    Mensaje = "Login exitoso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proceso de login: {Email}", request.Email);
                return new LoginCommandResponse
                {
                    Exitoso = false,
                    Mensaje = "Error en el proceso de autenticación"
                };
            }
        }
    }
}

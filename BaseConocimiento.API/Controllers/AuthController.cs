using BaseConocimiento.API.DTOs.Auth;
using BaseConocimiento.Application.Interfaces.Auth;
using BaseConocimiento.Application.UseCases.Auth.Commands;
using BaseConocimiento.Application.UseCases.Categorias.CrearCategoria;
using BaseConocimiento.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseConocimiento.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IJwtTokenGenerator _jwtGenerator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IMediator mediator,
            IJwtTokenGenerator jwtGenerator,
            ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _jwtGenerator = jwtGenerator;
            _logger = logger;
        }

        /// <summary>
        /// Login de usuario
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await _mediator.Send(command);

            if (!result.Exitoso)
            {
                return Unauthorized(new LoginResponse
                {
                    Exitoso = false,
                    Mensaje = result.Mensaje
                });
            }

            // Generar JWT
            var token = _jwtGenerator.GenerateToken(
                result.UsuarioId!.Value,
                result.Email!,
                result.NombreCompleto!,
                result.Rol!
            );

            return Ok(new LoginResponse
            {
                Exitoso = true,
                Token = token,
                Usuario = new UsuarioInfoDto
                {
                    Id = result.UsuarioId.Value,
                    NombreCompleto = result.NombreCompleto!,
                    Email = result.Email!,
                    Rol = result.Rol!
                },
                Mensaje = "Login exitoso"
            });
        }

        /// <summary>
        /// Registro de nuevo usuario
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var command = new CrearUsuarioCommand
            {
                NombreCompleto = request.NombreCompleto,
                Email = request.Email,
                Password = request.Password,
                Departamento = request.Departamento,
                Rol = RolUsuario.Lector
            };

            var response = await _mediator.Send(command);

            if (!response.Exitoso)
                return BadRequest(response);

            return Ok(new
            {
                exitoso = true,
                mensaje = "Usuario registrado exitosamente. Puede iniciar sesión.",
                usuarioId = response.UsuarioId
            });
        }

        /// <summary>
        /// Obtener información del usuario
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Ok(new
            {
                id = userId,
                email = email,
                nombreCompleto = nombre,
                rol = rol
            });
        }
    }
}

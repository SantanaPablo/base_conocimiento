using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Auth.Commands
{
    public class LoginCommand : IRequest<LoginCommandResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginCommandResponse
    {
        public bool Exitoso { get; set; }
        public Guid? UsuarioId { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Email { get; set; }
        public string? Rol { get; set; }
        public string Mensaje { get; set; }
    }
}

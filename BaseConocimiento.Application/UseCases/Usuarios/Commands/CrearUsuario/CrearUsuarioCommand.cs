using BaseConocimiento.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.CrearCategoria
{
    public class CrearUsuarioCommand : IRequest<CrearUsuarioResponse>
    {
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Departamento { get; set; }
        public RolUsuario Rol { get; set; } = RolUsuario.Lector;
    }

    public class CrearUsuarioResponse
    {
        public bool Exitoso { get; set; }
        public Guid? UsuarioId { get; set; }
        public string Mensaje { get; set; }
    }
}

using BaseConocimiento.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.ActualizarCategoria
{
    public class ActualizarUsuarioCommand : IRequest<ActualizarUsuarioResponse>
    {
        public Guid UsuarioId { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Departamento { get; set; }
        public RolUsuario? Rol { get; set; }
        public bool? EsActivo { get; set; }
    }

    public class ActualizarUsuarioResponse
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
    }
}

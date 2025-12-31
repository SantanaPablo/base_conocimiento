using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Usuarios.Queries.ListarUsuarios
{
    public class ListarUsuariosQuery : IRequest<ListarUsuariosResponse>
    {
        public bool SoloActivos { get; set; } = true;
        public string? Departamento { get; set; }
    }

    public class ListarUsuariosResponse
    {
        public bool Exitoso { get; set; }
        public List<UsuarioDto> Usuarios { get; set; } = new();
        public string Mensaje { get; set; }
    }

    public class UsuarioDto
    {
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string? Departamento { get; set; }
        public string Rol { get; set; }
        public bool EsActivo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public int TotalManualesSubidos { get; set; }
        public int TotalConsultas { get; set; }
    }
}

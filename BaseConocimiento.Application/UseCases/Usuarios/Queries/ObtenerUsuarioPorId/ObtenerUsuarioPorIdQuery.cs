using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.Queries.ObtenerCategoriaPorId
{
    public class ObtenerUsuarioPorIdQuery : IRequest<ObtenerUsuarioPorIdResponse>
    {
        public Guid UsuarioId { get; set; }
    }

    public class ObtenerUsuarioPorIdResponse
    {
        public bool Exitoso { get; set; }
        public UsuarioDetalleDto? Usuario { get; set; }
        public string Mensaje { get; set; }
    }

    public class UsuarioDetalleDto
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
        public DateTime? UltimaConsulta { get; set; }
    }
}

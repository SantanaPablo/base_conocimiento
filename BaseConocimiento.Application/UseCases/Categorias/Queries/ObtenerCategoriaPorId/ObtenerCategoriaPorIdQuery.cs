using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.Queries.ObtenerCategoriaPorId
{
    public class ObtenerCategoriaPorIdQuery : IRequest<ObtenerCategoriaPorIdResponse>
    {
        public Guid CategoriaId { get; set; }
    }

    public class ObtenerCategoriaPorIdResponse
    {
        public bool Exitoso { get; set; }
        public CategoriaDetalleDto? Categoria { get; set; }
        public string Mensaje { get; set; }
    }

    public class CategoriaDetalleDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? Color { get; set; }
        public string? Icono { get; set; }
        public int TotalManuales { get; set; }
        public int TotalSubcategorias { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}

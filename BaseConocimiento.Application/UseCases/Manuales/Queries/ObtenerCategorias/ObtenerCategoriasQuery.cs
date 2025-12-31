using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerCategorias
{
    public class ObtenerCategoriasQuery : IRequest<ObtenerCategoriasResponse>
    {
    }

    public class ObtenerCategoriasResponse
    {
        public List<CategoriaDto> Categorias { get; set; }
    }

    public class CategoriaDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string? Color { get; set; }
        public string? Icono { get; set; }
        public List<string> SubCategorias { get; set; }
        public int CantidadManuales { get; set; }
    }
}

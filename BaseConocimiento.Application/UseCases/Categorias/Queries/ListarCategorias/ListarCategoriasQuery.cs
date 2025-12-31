using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.Queries.ListarCategorias
{
    public class ListarCategoriasQuery : IRequest<ListarCategoriasResponse>
    {
        public bool SoloActivas { get; set; } = true;
        public bool IncluirSubcategorias { get; set; } = true;
    }

    public class ListarCategoriasResponse
    {
        public bool Exitoso { get; set; }
        public List<CategoriaDto> Categorias { get; set; } = new();
        public string Mensaje { get; set; }
    }

    public class CategoriaDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public Guid? CategoriaPadreId { get; set; }
        public string? Color { get; set; }
        public string? Icono { get; set; }
        public int Orden { get; set; }
        public bool EsActiva { get; set; }
        public int TotalManuales { get; set; }
        public List<CategoriaDto> SubCategorias { get; set; } = new();
    }
}

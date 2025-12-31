using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.Commands.CrearCategoria
{
    public class CrearCategoriaCommand : IRequest<CrearCategoriaResponse>
    {
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public Guid? CategoriaPadreId { get; set; }
        public string? Color { get; set; }
        public string? Icono { get; set; }
    }

    public class CrearCategoriaResponse
    {
        public bool Exitoso { get; set; }
        public Guid? CategoriaId { get; set; }
        public string Mensaje { get; set; }
    }
}

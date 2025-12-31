using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Domain.Entities
{
    public class Categoria
    {
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public string? Descripcion { get; private set; }
        public Guid? CategoriaPadreId { get; private set; }
        public string? Color { get; private set; }
        public string? Icono { get; private set; }
        public int Orden { get; private set; }
        public bool EsActiva { get; private set; }
        public DateTime FechaCreacion { get; private set; }

      
        public Categoria? CategoriaPadre { get; private set; }
        public ICollection<Categoria> SubCategorias { get; private set; } = new List<Categoria>();
        public ICollection<Manual> Manuales { get; private set; } = new List<Manual>();

        private Categoria() { }

        public static Categoria Crear(string nombre, string? descripcion = null, Guid? padreId = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es obligatorio", nameof(nombre));

            return new Categoria
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = descripcion,
                CategoriaPadreId = padreId,
                EsActiva = true,
                Orden = 0,
                FechaCreacion = DateTime.UtcNow
            };
        }

        public void ActualizarNombre(string nuevoNombre)
        {
            if (string.IsNullOrWhiteSpace(nuevoNombre))
                throw new ArgumentException("El nombre no puede estar vacío");
            Nombre = nuevoNombre;
        }

        public void AsignarColor(string color) => Color = color;
        public void AsignarIcono(string icono) => Icono = icono;
        public void CambiarOrden(int orden) => Orden = orden;
        public void Activar() => EsActiva = true;
        public void Desactivar() => EsActiva = false;
    }
}

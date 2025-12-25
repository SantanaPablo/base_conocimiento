using BaseConocimiento.Domain.Enums;
namespace BaseConocimiento.Domain.Entities
{
    public class Manual
    {
        public Guid Id { get; private set; }
        public string Titulo { get; private set; }
        public string Categoria { get; private set; }
        public string SubCategoria { get; private set; }
        public string Version { get; private set; }
        public string Descripcion { get; private set; }
        public string RutaLocal { get; private set; }
        public string NombreOriginal { get; private set; }
        public string UsuarioId { get; private set; }
        public DateTime FechaSubida { get; private set; }
        public long PesoArchivo { get; private set; }
        public EstadoManual Estado { get; private set; }

        // Constructor para crear un nuevo manual
        private Manual() { } // Para EF Core

        public static Manual Crear(
            string titulo,
            string categoria,
            string subCategoria,
            string version,
            string descripcion,
            string rutaLocal,
            string nombreOriginal,
            string usuarioId,
            long pesoArchivo)
        {
            // Validaciones de dominio
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título es obligatorio", nameof(titulo));

            if (string.IsNullOrWhiteSpace(categoria))
                throw new ArgumentException("La categoría es obligatoria", nameof(categoria));

            if (pesoArchivo <= 0)
                throw new ArgumentException("El peso del archivo debe ser mayor a 0", nameof(pesoArchivo));

            return new Manual
            {
                Id = Guid.NewGuid(),
                Titulo = titulo,
                Categoria = categoria,
                SubCategoria = subCategoria ?? string.Empty,
                Version = version ?? "v1.0",
                Descripcion = descripcion ?? string.Empty,
                RutaLocal = rutaLocal,
                NombreOriginal = nombreOriginal,
                UsuarioId = usuarioId,
                FechaSubida = DateTime.UtcNow,
                PesoArchivo = pesoArchivo,
                Estado = EstadoManual.Activo
            };
        }

        // Métodos de dominio
        public void ActualizarEstado(EstadoManual nuevoEstado)
        {
            Estado = nuevoEstado;
        }

        public void ActualizarVersion(string nuevaVersion)
        {
            if (string.IsNullOrWhiteSpace(nuevaVersion))
                throw new ArgumentException("La versión no puede estar vacía", nameof(nuevaVersion));

            Version = nuevaVersion;
        }

        public void ActualizarDescripcion(string nuevaDescripcion)
        {
            Descripcion = nuevaDescripcion ?? string.Empty;
        }

        public bool EstaObsoleto() => Estado == EstadoManual.Obsoleto;
        public bool EstaActivo() => Estado == EstadoManual.Activo;
    }
}
using BaseConocimiento.Domain.Enums;
namespace BaseConocimiento.Domain.Entities
{
    public class Manual
    {
        public Guid Id { get; private set; }
        public string Titulo { get; private set; }
        public Guid CategoriaId { get; private set; } 
        public string? SubCategoria { get; private set; }
        public string Version { get; private set; }
        public string Descripcion { get; private set; }
        public string RutaStorage { get; private set; }
        public string NombreOriginal { get; private set; }
        public Guid UsuarioId { get; private set; }
        public DateTime FechaSubida { get; private set; }
        public long PesoArchivo { get; private set; }
        public EstadoManual Estado { get; private set; }
        public int NumeroConsultas { get; private set; }
        public DateTime? UltimaConsulta { get; private set; }

        // Navegación
        public Categoria Categoria { get; private set; }
        public Usuario Usuario { get; private set; }
        public ICollection<ConsultaManual> ConsultasRelacionadas { get; private set; } = new List<ConsultaManual>();

        private Manual() { }

        public static Manual Crear(
            string titulo,
            Guid categoriaId,
            string version,
            string descripcion,
            string rutaStorage,
            string nombreOriginal,
            Guid usuarioId,
            long pesoArchivo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título es obligatorio", nameof(titulo));
            if (categoriaId == Guid.Empty)
                throw new ArgumentException("La categoría es obligatoria", nameof(categoriaId));
            if (pesoArchivo <= 0)
                throw new ArgumentException("El peso debe ser mayor a 0", nameof(pesoArchivo));

            return new Manual
            {
                Id = Guid.NewGuid(),
                Titulo = titulo,
                CategoriaId = categoriaId,
                Version = version ?? "v1.0",
                Descripcion = descripcion ?? string.Empty,
                RutaStorage = rutaStorage,
                NombreOriginal = nombreOriginal,
                UsuarioId = usuarioId,
                FechaSubida = DateTime.UtcNow,
                PesoArchivo = pesoArchivo,
                Estado = EstadoManual.Activo,
                NumeroConsultas = 0
            };
        }

        public void RegistrarConsulta()
        {
            NumeroConsultas++;
            UltimaConsulta = DateTime.UtcNow;
        }

        public void ActualizarEstado(EstadoManual nuevoEstado) => Estado = nuevoEstado;
        public void ActualizarVersion(string nuevaVersion)
        {
            if (string.IsNullOrWhiteSpace(nuevaVersion))
                throw new ArgumentException("La versión no puede estar vacía");
            Version = nuevaVersion;
        }
        public bool EstaActivo() => Estado == EstadoManual.Activo;

        public void ActualizarDescripcion(string nuevaDescripcion)
        {
            Descripcion = nuevaDescripcion ?? string.Empty;
        }
    }
}
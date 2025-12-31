using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Domain.Entities
{
    public class Usuario
    {
        public Guid Id { get; private set; }
        public string NombreCompleto { get; private set; }
        public string Email { get; private set; }
        public string? Departamento { get; private set; }
        public string PasswordHash { get; private set; }
        public RolUsuario Rol { get; private set; }
        public bool EsActivo { get; private set; }
        public DateTime FechaRegistro { get; private set; }
        public DateTime? UltimoAcceso { get; private set; }

        public ICollection<Manual> ManualesSubidos { get; private set; } = new List<Manual>();
        public ICollection<Consulta> ConsultasRealizadas { get; private set; } = new List<Consulta>();

        private Usuario() { }

        public static Usuario Crear(string nombreCompleto, string email, RolUsuario rol = RolUsuario.Lector)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                throw new ArgumentException("El nombre es obligatorio", nameof(nombreCompleto));
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Email inválido", nameof(email));

            return new Usuario
            {
                Id = Guid.NewGuid(),
                NombreCompleto = nombreCompleto,
                Email = email.ToLowerInvariant(),
                Rol = rol,
                EsActivo = true,
                FechaRegistro = DateTime.UtcNow
            };
        }

        public void ActualizarAcceso() => UltimoAcceso = DateTime.UtcNow;
        public void CambiarRol(RolUsuario nuevoRol) => Rol = nuevoRol;
        public void AsignarDepartamento(string departamento) => Departamento = departamento;
        public void Activar() => EsActivo = true;
        public void Desactivar() => EsActivo = false;
        public bool TienePermiso(RolUsuario rolRequerido) => Rol >= rolRequerido;
        public void AsignarPassword(string hash) => PasswordHash = hash;
    }

    public enum RolUsuario
    {
        Administrador = 1,
        Editor = 2,
        Lector = 3
    }
}

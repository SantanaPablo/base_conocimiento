using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BaseConocimiento.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly BaseConocimientoDbContext _context;

        public UsuarioRepository(BaseConocimientoDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        {
            // Busca por la PK definida en tu configuración
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<Usuario> ObtenerPorEmailAsync(string email, CancellationToken ct = default)
        {
            // El email es único según tu CategoriaConfiguration
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task<Usuario> ObtenerConDetallesAsync(Guid id, CancellationToken ct = default)
        {
            // Cargamos las relaciones definidas: ManualesSubidos y ConsultasRealizadas
            return await _context.Usuarios
                .Include(u => u.ManualesSubidos)
                .Include(u => u.ConsultasRealizadas)
                .FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<List<Usuario>> ListarConEstadisticasAsync(bool soloActivos, string departamento, CancellationToken ct = default)
        {
            var query = _context.Usuarios
                .Include(u => u.ManualesSubidos)
                .Include(u => u.ConsultasRealizadas)
                .AsQueryable();

            // Filtro por es_activo
            if (soloActivos)
            {
                query = query.Where(u => u.EsActivo);
            }

            // Filtro por departamento
            if (!string.IsNullOrWhiteSpace(departamento))
            {
                query = query.Where(u => u.Departamento == departamento);
            }

            return await query.ToListAsync(ct);
        }

        public async Task<List<Usuario>> ObtenerMasActivosAsync(int top, CancellationToken ct = default)
        {
            // Ordenamos por la cantidad de consultas realizadas
            return await _context.Usuarios
                .Include(u => u.ConsultasRealizadas)
                .OrderByDescending(u => u.ConsultasRealizadas.Count)
                .Take(top)
                .ToListAsync(ct);
        }

        public async Task<int> ContarActivosAsync(CancellationToken ct = default)
        {
            return await _context.Usuarios.CountAsync(u => u.EsActivo, ct);
        }

        public async Task AgregarAsync(Usuario usuario, CancellationToken ct = default)
        {
            await _context.Usuarios.AddAsync(usuario, ct);
        }

        public Task ActualizarAsync(Usuario usuario, CancellationToken ct = default)
        {
            _context.Usuarios.Update(usuario);
            return Task.CompletedTask;
        }
    }
}
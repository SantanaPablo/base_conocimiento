using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Domain.Enums;
using BaseConocimiento.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BaseConocimiento.Infrastructure.Repositories
{
    public class ManualRepository : IManualRepository
    {
        private readonly BaseConocimientoDbContext _context;

        public ManualRepository(BaseConocimientoDbContext context)
        {
            _context = context;
        }

        public async Task<Manual> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Manuales
                .FirstOrDefaultAsync(m => m.Id == id, ct);
        }

        public async Task<Manual> ObtenerConCategoriaAsync(Guid id, CancellationToken ct = default)
        {
            // Cargamos la relación Categoria definida en ManualConfiguration
            return await _context.Manuales
                .Include(m => m.Categoria)
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id, ct);
        }

        public async Task<(List<Manual> Manuales, int Total)> ListarPaginadoAsync(
            Guid? categoriaId,
            string terminoBusqueda,
            int pagina,
            int tamañoPagina,
            string ordenarPor,
            CancellationToken ct = default)
        {
            var query = _context.Manuales
                .Include(m => m.Categoria)
                .Include(m => m.Usuario)
                .AsQueryable();

            // Filtro por categoria_id
            if (categoriaId.HasValue)
            {
                query = query.Where(m => m.CategoriaId == categoriaId.Value);
            }

            // Búsqueda por título o descripción (usamos ILike para Postgres si está disponible o Contains)
            if (!string.IsNullOrWhiteSpace(terminoBusqueda))
            {
                query = query.Where(m => EF.Functions.ILike(m.Titulo, $"%{terminoBusqueda}%") ||
                                         EF.Functions.ILike(m.Descripcion, $"%{terminoBusqueda}%"));
            }

            var total = await query.CountAsync(ct);

            // Ordenamiento dinámico
            query = ordenarPor?.ToLower() switch
            {
                "titulo" => query.OrderBy(m => m.Titulo),
                "fecha" => query.OrderByDescending(m => m.FechaSubida),
                "consultas" => query.OrderByDescending(m => m.NumeroConsultas),
                _ => query.OrderByDescending(m => m.FechaSubida)
            };

            var items = await query
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<List<Manual>> ObtenerMasConsultadosAsync(int top, CancellationToken ct = default)
        {
            return await _context.Manuales
                .Include(m => m.Categoria)
                .OrderByDescending(m => m.NumeroConsultas)
                .Take(top)
                .ToListAsync(ct);
        }

        public async Task<int> ContarAsync(CancellationToken ct = default)
        {
            return await _context.Manuales.CountAsync(ct);
        }

        public async Task<int> ContarActivosAsync(CancellationToken ct = default)
        {
            // Estado se guarda como int según la configuración
            return await _context.Manuales
                .CountAsync(m => m.Estado == EstadoManual.Activo, ct);
        }

        public async Task<Manual?> ObtenerConDetallesAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Manuales
                .Include(m => m.Categoria)
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id, ct);
        }

        public async Task<Manual> AgregarAsync(Manual manual, CancellationToken ct = default)
        {
            await _context.Manuales.AddAsync(manual, ct);
            return manual;
        }

        public Task ActualizarAsync(Manual manual, CancellationToken ct = default)
        {
            _context.Manuales.Update(manual);
            return Task.CompletedTask;
        }

        public Task EliminarAsync(Manual manual, CancellationToken ct = default)
        {
            _context.Manuales.Remove(manual);
            return Task.CompletedTask;
        }
    }
}
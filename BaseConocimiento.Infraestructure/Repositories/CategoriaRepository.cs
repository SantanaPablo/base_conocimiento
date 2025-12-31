using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BaseConocimiento.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly BaseConocimientoDbContext _context;

        public CategoriaRepository(BaseConocimientoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Categoria>> ObtenerTodasAsync(CancellationToken ct = default)
        {
            return await _context.Categorias
                .Include(c => c.SubCategorias)
                .Where(c => c.EsActiva)
                .OrderBy(c => c.Orden)
                .ToListAsync(ct);
        }

        public async Task<Categoria> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<Categoria?> ObtenerConDetallesAsync(Guid id, bool incluirSubcategorias = false, CancellationToken ct = default)
        {
            var query = _context.Categorias
                .Include(c => c.Manuales)
                .AsQueryable();

            if (incluirSubcategorias)
            {
                query = query.Include(c => c.SubCategorias);
            }

            return await query.FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<List<dynamic>> ObtenerEstadisticasManualesAsync(CancellationToken ct = default)
        {
            return await _context.Categorias
                .Select(c => new
                {
                    Categoria = c.Nombre,
                    TotalManuales = c.Manuales.Count,
                    TotalConsultas = c.Manuales.Sum(m => m.NumeroConsultas)
                })
                .Cast<dynamic>()
                .ToListAsync(ct);
        }

        public async Task<Categoria?> ObtenerPorNombreAsync(string nombre, CancellationToken ct = default)
        {
       
            return await _context.Categorias
                .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Nombre, nombre), ct);
        }

        public async Task<List<Categoria>> ListarConDetallesAsync(bool soloActivas = false, bool incluirSubcategorias = false, CancellationToken ct = default)
        {
            var query = _context.Categorias
                .Include(c => c.Manuales)
                .AsQueryable();

            if (soloActivas)
            {
                query = query.Where(c => c.EsActiva);
            }

            if (incluirSubcategorias)
            {
                query = query.Include(c => c.SubCategorias);
            }

            return await query.OrderBy(c => c.Orden).ToListAsync(ct);
        }

        public async Task AgregarAsync(Categoria categoria, CancellationToken ct = default)
        {
            await _context.Categorias.AddAsync(categoria, ct);
        }
    }
}
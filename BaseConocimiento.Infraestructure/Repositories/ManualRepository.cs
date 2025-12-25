using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Domain.Enums;
using BaseConocimiento.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Infrastructure.Repositories
{
    public class ManualRepository : IManualRepository
    {
        private readonly BaseConocimientoDbContext _context;

        public ManualRepository(BaseConocimientoDbContext context)
        {
            _context = context;
        }

        public async Task<Manual> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Manuales
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<List<Manual>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Manuales
                .OrderByDescending(m => m.FechaSubida)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Manual>> ObtenerPorCategoriaAsync(string categoria, CancellationToken cancellationToken = default)
        {
            return await _context.Manuales
                .Where(m => m.Categoria == categoria)
                .OrderByDescending(m => m.FechaSubida)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Manual>> ObtenerPorEstadoAsync(EstadoManual estado, CancellationToken cancellationToken = default)
        {
            return await _context.Manuales
                .Where(m => m.Estado == estado)
                .OrderByDescending(m => m.FechaSubida)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<Manual> Manuales, int Total)> ObtenerPaginadoAsync(
            int pagina,
            int tamañoPagina,
            string categoria = null,
            string subCategoria = null,
            EstadoManual? estado = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Manuales.AsQueryable();

            if (!string.IsNullOrEmpty(categoria))
                query = query.Where(m => m.Categoria == categoria);

            if (!string.IsNullOrEmpty(subCategoria))
                query = query.Where(m => m.SubCategoria == subCategoria);

            if (estado.HasValue)
                query = query.Where(m => m.Estado == estado.Value);

            var total = await query.CountAsync(cancellationToken);

            var manuales = await query
                .OrderByDescending(m => m.FechaSubida)
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync(cancellationToken);

            return (manuales, total);
        }

        public async Task<Manual> AgregarAsync(Manual manual, CancellationToken cancellationToken = default)
        {
            await _context.Manuales.AddAsync(manual, cancellationToken);
            return manual;
        }

        public Task ActualizarAsync(Manual manual, CancellationToken cancellationToken = default)
        {
            _context.Manuales.Update(manual);
            return Task.CompletedTask;
        }

        public Task EliminarAsync(Manual manual, CancellationToken cancellationToken = default)
        {
            _context.Manuales.Remove(manual);
            return Task.CompletedTask;
        }

        public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Manuales
                .AnyAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<Dictionary<string, List<string>>> ObtenerCategoriasYSubcategoriasAsync(
            CancellationToken cancellationToken = default)
        {
            var manuales = await _context.Manuales
                .Where(m => m.Estado == EstadoManual.Activo)
                .Select(m => new { m.Categoria, m.SubCategoria })
                .Distinct()
                .ToListAsync(cancellationToken);

            return manuales
                .GroupBy(m => m.Categoria)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.SubCategoria)
                          .Where(s => !string.IsNullOrEmpty(s))
                          .Distinct()
                          .ToList()
                );
        }
    }
}

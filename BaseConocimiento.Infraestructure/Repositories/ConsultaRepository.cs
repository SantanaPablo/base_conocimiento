using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BaseConocimiento.Infrastructure.Repositories
{
    public class ConsultaRepository : IConsultaRepository
    {
        private readonly BaseConocimientoDbContext _context;

        public ConsultaRepository(BaseConocimientoDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(Consulta consulta, CancellationToken ct = default)
        {
            await _context.Consultas.AddAsync(consulta, ct);
        }

        public async Task<(List<Consulta> Consultas, int Total)> ListarPaginadoAsync(
            Guid? usuarioId,
            int pagina,
            int tamañoPagina,
            CancellationToken ct = default)
        {
            var query = _context.Consultas
                .Include(c => c.Usuario)
                .Include(c => c.ManualesConsultados)
                .AsQueryable();

            // Filtro por usuario_id según tu configuración
            if (usuarioId.HasValue)
            {
                query = query.Where(c => c.UsuarioId == usuarioId.Value);
            }

            var total = await query.CountAsync(ct);

            // Ordenamos por fecha_consulta de forma descendente (lo más nuevo primero)
            var items = await query
                .OrderByDescending(c => c.FechaConsulta)
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<int> ContarAsync(CancellationToken ct = default)
        {
            return await _context.Consultas.CountAsync(ct);
        }

        public async Task<int> ContarPorFechaAsync(DateTime fecha, CancellationToken ct = default)
        {
            // Filtramos comparando solo la parte de la fecha (Date) en la columna fecha_consulta
            return await _context.Consultas
                .CountAsync(c => c.FechaConsulta.Date == fecha.Date, ct);
        }

        public async Task<double> ObtenerTiempoPromedioRespuestaAsync(CancellationToken ct = default)
        {
            // Calculamos el promedio de la columna tiempo_respuesta_ms
            if (!await _context.Consultas.AnyAsync(ct))
                return 0;

            return await _context.Consultas
                .AverageAsync(c => c.TiempoRespuestaMs, ct);
        }
    }
}
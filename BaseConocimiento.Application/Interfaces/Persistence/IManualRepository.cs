using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.Interfaces.Persistence
{
    public interface IManualRepository
    {
        Task<Manual> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Manual>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        Task<List<Manual>> ObtenerPorCategoriaAsync(string categoria, CancellationToken cancellationToken = default);
        Task<List<Manual>> ObtenerPorEstadoAsync(EstadoManual estado, CancellationToken cancellationToken = default);
        Task<(List<Manual> Manuales, int Total)> ObtenerPaginadoAsync(
            int pagina,
            int tamañoPagina,
            string categoria = null,
            string subCategoria = null,
            EstadoManual? estado = null,
            CancellationToken cancellationToken = default);
        Task<Manual> AgregarAsync(Manual manual, CancellationToken cancellationToken = default);
        Task ActualizarAsync(Manual manual, CancellationToken cancellationToken = default);
        Task EliminarAsync(Manual manual, CancellationToken cancellationToken = default);
        Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Dictionary<string, List<string>>> ObtenerCategoriasYSubcategoriasAsync(CancellationToken cancellationToken = default);
    }
}

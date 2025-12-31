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
        Task<Manual> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
        Task<Manual> ObtenerConCategoriaAsync(Guid id, CancellationToken ct = default);

        Task<Manual?> ObtenerConDetallesAsync(Guid id, CancellationToken ct = default);
        Task<(List<Manual> Manuales, int Total)> ListarPaginadoAsync(
            Guid? categoriaId,
            string terminoBusqueda,
            int pagina,
            int tamañoPagina,
            string ordenarPor,
            CancellationToken ct = default);

        Task<List<Manual>> ObtenerMasConsultadosAsync(int top, CancellationToken ct = default);
        Task<int> ContarAsync(CancellationToken ct = default);
        Task<int> ContarActivosAsync(CancellationToken ct = default);

        Task<Manual> AgregarAsync(Manual manual, CancellationToken ct = default);
        Task ActualizarAsync(Manual manual, CancellationToken ct = default);
        Task EliminarAsync(Manual manual, CancellationToken ct = default);
    }
}

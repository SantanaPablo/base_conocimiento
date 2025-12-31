using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.Interfaces.Persistence
{
    public interface ICategoriaRepository
    {
        Task<List<Categoria>> ObtenerTodasAsync(CancellationToken ct = default);
        Task<Categoria> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
        Task<Categoria?> ObtenerPorNombreAsync(string nombre, CancellationToken ct = default);
        Task<List<Categoria>> ListarConDetallesAsync(bool soloActivas = false, bool incluirSubcategorias = false, CancellationToken ct = default);
        Task<Categoria?> ObtenerConDetallesAsync(Guid id, bool incluirSubcategorias = false, CancellationToken ct = default);
        Task<List<dynamic>> ObtenerEstadisticasManualesAsync(CancellationToken ct = default);

        Task AgregarAsync(Categoria categoria, CancellationToken ct = default);
    }
}

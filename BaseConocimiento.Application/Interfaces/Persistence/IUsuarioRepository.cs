using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.Interfaces.Persistence
{
    public interface IUsuarioRepository
    {
        Task<Usuario> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
        Task<Usuario> ObtenerPorEmailAsync(string email, CancellationToken ct = default);
        Task<Usuario> ObtenerConDetallesAsync(Guid id, CancellationToken ct = default);
        Task<List<Usuario>> ListarConEstadisticasAsync(bool soloActivos, string departamento, CancellationToken ct = default);
        Task<List<Usuario>> ObtenerMasActivosAsync(int top, CancellationToken ct = default);
        Task<int> ContarActivosAsync(CancellationToken ct = default);

        Task AgregarAsync(Usuario usuario, CancellationToken ct = default);
        Task ActualizarAsync(Usuario usuario, CancellationToken ct = default);
    }
}

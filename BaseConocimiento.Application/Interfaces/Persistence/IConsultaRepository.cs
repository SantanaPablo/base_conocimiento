using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.Interfaces.Persistence
{
    public interface IConsultaRepository
    {
        Task AgregarAsync(Consulta consulta, CancellationToken ct = default);

        Task<(List<Consulta> Consultas, int Total)> ListarPaginadoAsync(
            Guid? usuarioId,
            int pagina,
            int tamañoPagina,
            CancellationToken ct = default);

        Task<int> ContarAsync(CancellationToken ct = default);
        Task<int> ContarPorFechaAsync(DateTime fecha, CancellationToken ct = default);
        Task<double> ObtenerTiempoPromedioRespuestaAsync(CancellationToken ct = default);
    }
}

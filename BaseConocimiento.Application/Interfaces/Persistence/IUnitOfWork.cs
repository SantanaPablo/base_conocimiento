using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.Interfaces.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IManualRepository Manuales { get; }
        IUsuarioRepository Usuarios { get; }
        ICategoriaRepository Categorias { get; }
        IConsultaRepository Consultas { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task ExecuteStrategyAsync(Func<Task> action);

        Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}

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
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task ExecuteStrategyAsync(Func<Task> action);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}

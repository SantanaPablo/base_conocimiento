using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Domain.Enums;
using BaseConocimiento.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BaseConocimientoDbContext _context;
        private IDbContextTransaction _transaction;
        private IManualRepository _manuales;

        public UnitOfWork(BaseConocimientoDbContext context)
        {
            _context = context;
        }

        public IManualRepository Manuales
        {
            get
            {
                _manuales ??= new ManualRepository(_context);
                return _manuales;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await SaveChangesAsync(cancellationToken);
                await _transaction?.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _transaction?.RollbackAsync(cancellationToken);
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }

        public async Task ExecuteStrategyAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(action);
        }
    }
}

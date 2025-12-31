using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Domain.Enums;
using BaseConocimiento.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BaseConocimiento.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BaseConocimientoDbContext _context;
        private IDbContextTransaction? _transaction;

        private IManualRepository? _manuales;
        private IUsuarioRepository? _usuarios;
        private ICategoriaRepository? _categorias;
        private IConsultaRepository? _consultas;

        public UnitOfWork(BaseConocimientoDbContext context)
        {
            _context = context;
        }

        public IManualRepository Manuales => _manuales ??= new ManualRepository(_context);
        public IUsuarioRepository Usuarios => _usuarios ??= new UsuarioRepository(_context);
        public ICategoriaRepository Categorias => _categorias ??= new CategoriaRepository(_context);
        public IConsultaRepository Consultas => _consultas ??= new ConsultaRepository(_context);

        // Guardado de cambios
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task ExecuteStrategyAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(action);
        }

        public async Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return _transaction;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await SaveChangesAsync(cancellationToken);
                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                }
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
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
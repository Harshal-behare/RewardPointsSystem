using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Infrastructure.Data;

namespace RewardPointsSystem.Infrastructure.Repositories
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly RewardPointsDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed = false;

        private IRepository<User> _users;
        private IRepository<Role> _roles;
        private IRepository<UserRole> _userRoles;
        private IRepository<Event> _events;
        private IRepository<EventParticipant> _eventParticipants;
        private IRepository<UserPointsAccount> _userPointsAccounts;
        private IRepository<UserPointsTransaction> _userPointsTransactions;
        private IRepository<Product> _products;
        private IRepository<ProductPricing> _productPricings;
        private IRepository<InventoryItem> _inventoryItems;
        private IRepository<Redemption> _redemptions;

        public EfUnitOfWork(RewardPointsDbContext context)
        {
            _context = context;
        }

        // Core repositories
        public IRepository<User> Users => _users ??= new EfRepository<User>(_context);
        public IRepository<Role> Roles => _roles ??= new EfRepository<Role>(_context);
        public IRepository<UserRole> UserRoles => _userRoles ??= new EfRepository<UserRole>(_context);

        // Event repositories
        public IRepository<Event> Events => _events ??= new EfRepository<Event>(_context);
        public IRepository<EventParticipant> EventParticipants => _eventParticipants ??= new EfRepository<EventParticipant>(_context);

        // Account repositories
        public IRepository<UserPointsAccount> UserPointsAccounts => _userPointsAccounts ??= new EfRepository<UserPointsAccount>(_context);
        public IRepository<UserPointsTransaction> UserPointsTransactions => _userPointsTransactions ??= new EfRepository<UserPointsTransaction>(_context);

        // Product repositories
        public IRepository<Product> Products => _products ??= new EfRepository<Product>(_context);
        public IRepository<ProductPricing> Pricing => _productPricings ??= new EfRepository<ProductPricing>(_context);
        public IRepository<InventoryItem> Inventory => _inventoryItems ??= new EfRepository<InventoryItem>(_context);

        // Operation repositories
        public IRepository<Redemption> Redemptions => _redemptions ??= new EfRepository<Redemption>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
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

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

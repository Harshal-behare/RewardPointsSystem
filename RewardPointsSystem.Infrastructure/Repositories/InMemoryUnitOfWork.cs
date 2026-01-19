using System;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Domain.Entities.Operations;

namespace RewardPointsSystem.Infrastructure.Repositories
{
    public class InMemoryUnitOfWork : IUnitOfWork
    {
        private readonly Lazy<IRepository<User>> _users;
        private readonly Lazy<IRepository<Role>> _roles;
        private readonly Lazy<IRepository<UserRole>> _userRoles;
        private readonly Lazy<IRepository<Event>> _events;
        private readonly Lazy<IRepository<EventParticipant>> _eventParticipants;
        private readonly Lazy<IRepository<UserPointsAccount>> _userPointsAccounts;
        private readonly Lazy<IRepository<UserPointsTransaction>> _userPointsTransactions;
        private readonly Lazy<IRepository<Product>> _products;
        private readonly Lazy<IRepository<ProductPricing>> _productPricings;
        private readonly Lazy<IRepository<InventoryItem>> _inventoryItems;
        private readonly Lazy<IRepository<ProductCategory>> _productCategories;
        private readonly Lazy<IRepository<Redemption>> _redemptions;

        private bool _disposed = false;
        private bool _inTransaction = false;

        public InMemoryUnitOfWork()
        {
            _users = new Lazy<IRepository<User>>(() => new InMemoryRepository<User>());
            _roles = new Lazy<IRepository<Role>>(() => new InMemoryRepository<Role>());
            _userRoles = new Lazy<IRepository<UserRole>>(() => new InMemoryUserRoleRepository());
            _events = new Lazy<IRepository<Event>>(() => new InMemoryRepository<Event>());
            _eventParticipants = new Lazy<IRepository<EventParticipant>>(() => new InMemoryRepository<EventParticipant>());
            _userPointsAccounts = new Lazy<IRepository<UserPointsAccount>>(() => new InMemoryRepository<UserPointsAccount>());
            _userPointsTransactions = new Lazy<IRepository<UserPointsTransaction>>(() => new InMemoryRepository<UserPointsTransaction>());
            _products = new Lazy<IRepository<Product>>(() => new InMemoryRepository<Product>());
            _productPricings = new Lazy<IRepository<ProductPricing>>(() => new InMemoryRepository<ProductPricing>());
            _inventoryItems = new Lazy<IRepository<InventoryItem>>(() => new InMemoryRepository<InventoryItem>());
            _productCategories = new Lazy<IRepository<ProductCategory>>(() => new InMemoryRepository<ProductCategory>());
            _redemptions = new Lazy<IRepository<Redemption>>(() => new InMemoryRepository<Redemption>());
        }

        // Core repositories
        public IRepository<User> Users => _users.Value;
        public IRepository<Role> Roles => _roles.Value;
        public IRepository<UserRole> UserRoles => _userRoles.Value;

        // Event repositories
        public IRepository<Event> Events => _events.Value;
        public IRepository<EventParticipant> EventParticipants => _eventParticipants.Value;

        // Account repositories
        public IRepository<UserPointsAccount> UserPointsAccounts => _userPointsAccounts.Value;
        public IRepository<UserPointsTransaction> UserPointsTransactions => _userPointsTransactions.Value;

        // Product repositories
        public IRepository<Product> Products => _products.Value;
        public IRepository<ProductPricing> Pricing => _productPricings.Value;
        public IRepository<InventoryItem> Inventory => _inventoryItems.Value;
        public IRepository<ProductCategory> ProductCategories => _productCategories.Value;

        // Operation repositories
        public IRepository<Redemption> Redemptions => _redemptions.Value;

        public Task<int> SaveChangesAsync()
        {
            // In-memory implementation doesn't need explicit saves
            // Changes are persisted immediately in memory
            return Task.FromResult(0);
        }

        public Task BeginTransactionAsync()
        {
            _inTransaction = true;
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync()
        {
            if (!_inTransaction)
            {
                throw new InvalidOperationException("No active transaction to commit");
            }
            
            _inTransaction = false;
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync()
        {
            if (!_inTransaction)
            {
                throw new InvalidOperationException("No active transaction to rollback");
            }
            
            // Note: In a real implementation, you would rollback changes
            // For in-memory, we'll just clear the transaction state
            _inTransaction = false;
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
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
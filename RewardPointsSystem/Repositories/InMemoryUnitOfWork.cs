using System;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models;

namespace RewardPointsSystem.Repositories
{
    public class InMemoryUnitOfWork : IUnitOfWork
    {
        private IRepository<User> _users;
        private IRepository<Product> _products;
        private IRepository<Event> _events;
        private IRepository<Redemption> _redemptions;
        private IRepository<PointsTransaction> _pointsTransactions;
        private IRepository<Role> _roles;
        private IRepository<InventoryItem> _inventoryItems;

        public IRepository<User> Users => _users ??= new InMemoryRepository<User>();
        public IRepository<Product> Products => _products ??= new InMemoryRepository<Product>();
        public IRepository<Event> Events => _events ??= new InMemoryRepository<Event>();
        public IRepository<Redemption> Redemptions => _redemptions ??= new InMemoryRepository<Redemption>();
        public IRepository<PointsTransaction> PointsTransactions => _pointsTransactions ??= new InMemoryRepository<PointsTransaction>();
        public IRepository<Role> Roles => _roles ??= new InMemoryRepository<Role>();
        public IRepository<InventoryItem> InventoryItems => _inventoryItems ??= new InMemoryRepository<InventoryItem>();

        public int Complete()
        {
            // In an in-memory implementation, changes are immediate
            // This method would save changes in a real database implementation
            // Return 1 to indicate success
            return 1;
        }

        public void Dispose()
        {
            // Nothing to dispose in the in-memory implementation
            // In a real database implementation, this would dispose of the context
        }
    }
}
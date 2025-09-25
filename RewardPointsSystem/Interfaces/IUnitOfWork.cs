using System;
using RewardPointsSystem.Models;

namespace RewardPointsSystem.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Product> Products { get; }
        IRepository<Event> Events { get; }
        IRepository<Redemption> Redemptions { get; }
        IRepository<PointsTransaction> PointsTransactions { get; }
        IRepository<Role> Roles { get; }
        IRepository<InventoryItem> InventoryItems { get; }
        
        int Complete();
    }
}
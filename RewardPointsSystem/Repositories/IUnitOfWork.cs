using System;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Core;
using RewardPointsSystem.Models.Events;
using RewardPointsSystem.Models.Accounts;
using RewardPointsSystem.Models.Products;
using RewardPointsSystem.Models.Operations;

namespace RewardPointsSystem.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Core repositories
        IRepository<User> Users { get; }
        IRepository<Role> Roles { get; }
        IRepository<UserRole> UserRoles { get; }

        // Event repositories
        IRepository<Event> Events { get; }
        IRepository<EventParticipant> EventParticipants { get; }

        // Account repositories
        IRepository<RewardAccount> RewardAccounts { get; }
        IRepository<PointsTransaction> Transactions { get; }

        // Product repositories
        IRepository<Product> Products { get; }
        IRepository<ProductPricing> Pricing { get; }
        IRepository<InventoryItem> Inventory { get; }

        // Operation repositories
        IRepository<Redemption> Redemptions { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
using System;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Domain.Entities.Operations;

namespace RewardPointsSystem.Application.Interfaces
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
        IRepository<UserPointsAccount> UserPointsAccounts { get; }
        IRepository<UserPointsTransaction> UserPointsTransactions { get; }

        // Product repositories
        IRepository<Product> Products { get; }
        IRepository<ProductPricing> Pricing { get; }
        IRepository<InventoryItem> Inventory { get; }
        IRepository<ProductCategory> ProductCategories { get; }

        // Operation repositories
        IRepository<Redemption> Redemptions { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
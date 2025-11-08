using Microsoft.EntityFrameworkCore;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Domain.Entities.Products;

namespace RewardPointsSystem.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for the Reward Points System
    /// </summary>
    public class RewardPointsDbContext : DbContext
    {
        public RewardPointsDbContext(DbContextOptions<RewardPointsDbContext> options)
            : base(options)
        {
        }

        // Core Entities
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        // Account Entities
        public DbSet<PointsAccount> PointsAccounts { get; set; }
        public DbSet<PointsTransaction> PointsTransactions { get; set; }

        // Event Entities
        public DbSet<Event> Events { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }

        // Product Entities
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductPricing> ProductPricings { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }

        // Operation Entities
        public DbSet<Redemption> Redemptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User Entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Self-referencing foreign key for UpdatedBy
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UpdatedBy)
                    .OnDelete(DeleteBehavior.NoAction);

                // Unique constraint on Email
                entity.HasIndex(e => e.Email).IsUnique();

                // One-to-One relationship with PointsAccount
                entity.HasOne(e => e.PointsAccount)
                    .WithOne(p => p.User)
                    .HasForeignKey<PointsAccount>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // One-to-Many with UserRoles
                entity.HasMany(e => e.UserRoles)
                    .WithOne(ur => ur.User)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // One-to-Many with EventParticipations
                entity.HasMany(e => e.EventParticipations)
                    .WithOne(ep => ep.User)
                    .HasForeignKey(ep => ep.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // One-to-Many with Redemptions
                entity.HasMany(e => e.Redemptions)
                    .WithOne(r => r.User)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Role Entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Unique constraint on Role Name
                entity.HasIndex(e => e.Name).IsUnique();

                // One-to-Many with UserRoles
                entity.HasMany(e => e.UserRoles)
                    .WithOne(ur => ur.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserRole Entity (Composite Key)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
                entity.Property(e => e.AssignedAt).IsRequired();
                entity.Property(e => e.AssignedBy).IsRequired();

                // Relationships configured in User and Role entities
            });

            // Configure PointsAccount Entity
            modelBuilder.Entity<PointsAccount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.CurrentBalance).IsRequired();
                entity.Property(e => e.TotalEarned).IsRequired();
                entity.Property(e => e.TotalRedeemed).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.LastUpdatedAt).IsRequired();

                // Check constraints for data integrity
                entity.HasCheckConstraint("CK_PointsAccount_CurrentBalance", "[CurrentBalance] >= 0");
                entity.HasCheckConstraint("CK_PointsAccount_TotalEarned", "[TotalEarned] >= 0");
                entity.HasCheckConstraint("CK_PointsAccount_TotalRedeemed", "[TotalRedeemed] >= 0");

                // Foreign key for UpdatedBy
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UpdatedBy)
                    .OnDelete(DeleteBehavior.NoAction);

                // Unique constraint on UserId
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            // Configure PointsTransaction Entity
            modelBuilder.Entity<PointsTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Points).IsRequired();
                entity.Property(e => e.TransactionType).IsRequired().HasConversion<string>();
                entity.Property(e => e.TransactionSource).IsRequired().HasConversion<string>();
                entity.Property(e => e.SourceId).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.BalanceAfter).IsRequired();

                // Relationship with User
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Composite index for balance reconciliation and audit trail
                // Includes BalanceAfter and Points for covering index performance
                entity.HasIndex(e => new { e.UserId, e.Timestamp })
                    .IsDescending(false, true)
                    .IncludeProperties(e => new { e.BalanceAfter, e.Points });
                
                entity.HasIndex(e => e.Timestamp);
            });

            // Configure Event Entity
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.EventDate).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasConversion<string>();
                entity.Property(e => e.TotalPointsPool).IsRequired();
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.VirtualLink).HasMaxLength(1000);
                entity.Property(e => e.BannerImageUrl).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Check constraint for data integrity
                entity.HasCheckConstraint("CK_Event_TotalPointsPool", "[TotalPointsPool] > 0");

                // Relationship with User (Creator)
                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                // One-to-Many with EventParticipants
                entity.HasMany(e => e.Participants)
                    .WithOne(ep => ep.Event)
                    .HasForeignKey(ep => ep.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index on EventDate and Status
                entity.HasIndex(e => e.EventDate);
                entity.HasIndex(e => e.Status);
            });

            // Configure EventParticipant Entity
            modelBuilder.Entity<EventParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventId).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.AttendanceStatus).IsRequired().HasConversion<string>();
                entity.Property(e => e.RegisteredAt).IsRequired();

                // Unique constraint - a user can only participate once per event
                entity.HasIndex(e => new { e.EventId, e.UserId }).IsUnique();
                
                // Index on attendance status for reporting
                entity.HasIndex(e => e.AttendanceStatus);
            });

            // Configure ProductCategory Entity
            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.DisplayOrder).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();

                // Unique constraint on Name
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.DisplayOrder);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Product Entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();

                // Relationship with User (Creator)
                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship with ProductCategory
                entity.HasOne(e => e.ProductCategory)
                    .WithMany(pc => pc.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                // One-to-Many with ProductPricing
                entity.HasMany(e => e.PricingHistory)
                    .WithOne(pp => pp.Product)
                    .HasForeignKey(pp => pp.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                // One-to-One with InventoryItem
                entity.HasOne(e => e.Inventory)
                    .WithOne(i => i.Product)
                    .HasForeignKey<InventoryItem>(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                // One-to-Many with Redemptions
                entity.HasMany(e => e.Redemptions)
                    .WithOne(r => r.Product)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure ProductPricing Entity
            modelBuilder.Entity<ProductPricing>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.PointsCost).IsRequired();
                entity.Property(e => e.EffectiveFrom).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();

                // Check constraint for data integrity
                entity.HasCheckConstraint("CK_ProductPricing_PointsCost", "[PointsCost] > 0");

                // Composite index for optimal current price queries
                // Optimizes: SELECT * FROM ProductPricings WHERE ProductId = @id AND IsActive = 1 ORDER BY EffectiveFrom DESC
                entity.HasIndex(e => new { e.ProductId, e.IsActive, e.EffectiveFrom })
                    .IsDescending(false, false, true);
                
                entity.HasIndex(e => e.EffectiveFrom);
            });

            // Configure InventoryItem Entity
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.QuantityAvailable).IsRequired();
                entity.Property(e => e.QuantityReserved).IsRequired();
                entity.Property(e => e.ReorderLevel).IsRequired();
                entity.Property(e => e.LastRestocked).IsRequired();
                entity.Property(e => e.LastUpdated).IsRequired();

                // Check constraints for data integrity
                entity.HasCheckConstraint("CK_InventoryItem_QuantityAvailable", "[QuantityAvailable] >= 0");
                entity.HasCheckConstraint("CK_InventoryItem_QuantityReserved", "[QuantityReserved] >= 0");

                // Foreign key for UpdatedBy
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UpdatedBy)
                    .OnDelete(DeleteBehavior.NoAction);

                // Unique constraint on ProductId
                entity.HasIndex(e => e.ProductId).IsUnique();
            });

            // Configure Redemption Entity
            modelBuilder.Entity<Redemption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.PointsSpent).IsRequired();
                entity.Property(e => e.Quantity).IsRequired().HasDefaultValue(1);
                entity.Property(e => e.Status).IsRequired().HasConversion<string>();
                entity.Property(e => e.RequestedAt).IsRequired();
                entity.Property(e => e.DeliveryNotes).HasMaxLength(1000);
                entity.Property(e => e.RejectionReason).HasMaxLength(500);

                // Check constraint for data integrity
                entity.HasCheckConstraint("CK_Redemption_Quantity", "[Quantity] > 0");

                // Relationship with User (Approver)
                entity.HasOne(e => e.Approver)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.NoAction);

                // Relationship with User (Processor)
                entity.HasOne(e => e.Processor)
                    .WithMany()
                    .HasForeignKey(e => e.ProcessedBy)
                    .OnDelete(DeleteBehavior.NoAction);

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RequestedAt);
                entity.HasIndex(e => e.ApprovedBy);
                entity.HasIndex(e => e.ProcessedBy);
            });
        }
    }
}

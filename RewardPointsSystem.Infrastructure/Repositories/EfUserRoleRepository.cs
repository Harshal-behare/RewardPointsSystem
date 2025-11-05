using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Infrastructure.Data;
using System.Reflection;

namespace RewardPointsSystem.Infrastructure.Repositories
{
    public class EfUserRoleRepository : EfRepository<UserRole>
    {
        public EfUserRoleRepository(RewardPointsDbContext context) : base(context)
        {
        }

        public new async Task<UserRole> GetByIdAsync(Guid id)
        {
            // UserRole uses composite key, so we need to handle this differently
            // For now, we'll use FindAsync with composite key (UserId, RoleId)
            throw new NotSupportedException("UserRole uses composite key. Use FindAsync with predicate instead.");
        }

        public new async Task DeleteAsync(Guid id)
        {
            // UserRole uses composite key, so we need to handle this differently
            throw new NotSupportedException("UserRole uses composite key. Use DeleteAsync with entity instead.");
        }
    }
}

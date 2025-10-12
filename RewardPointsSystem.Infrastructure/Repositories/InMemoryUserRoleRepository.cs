using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Specialized repository for UserRole entity which uses composite key (UserId + RoleId)
    /// </summary>
    public class InMemoryUserRoleRepository : IRepository<UserRole>
    {
        private readonly ConcurrentDictionary<string, UserRole> _entities;

        public InMemoryUserRoleRepository()
        {
            _entities = new ConcurrentDictionary<string, UserRole>();
        }

        private string GetCompositeKey(UserRole userRole)
        {
            return $"{userRole.UserId}_{userRole.RoleId}";
        }

        private string GetCompositeKey(Guid userId, Guid roleId)
        {
            return $"{userId}_{roleId}";
        }

        public Task<UserRole> GetByIdAsync(Guid id)
        {
            // For UserRole, we don't use a single Id, so return null
            // Users should use FindAsync or SingleOrDefaultAsync instead
            return Task.FromResult<UserRole>(null);
        }

        public Task<IEnumerable<UserRole>> GetAllAsync()
        {
            return Task.FromResult(_entities.Values.AsEnumerable());
        }

        public Task<IEnumerable<UserRole>> FindAsync(Expression<Func<UserRole, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            var result = _entities.Values.Where(compiledPredicate);
            return Task.FromResult(result);
        }

        public Task<UserRole> SingleOrDefaultAsync(Expression<Func<UserRole, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            var result = _entities.Values.SingleOrDefault(compiledPredicate);
            return Task.FromResult(result);
        }

        public Task<bool> ExistsAsync(Expression<Func<UserRole, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            var result = _entities.Values.Any(compiledPredicate);
            return Task.FromResult(result);
        }

        public Task<int> CountAsync(Expression<Func<UserRole, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return Task.FromResult(_entities.Count);
            }

            var compiledPredicate = predicate.Compile();
            var result = _entities.Values.Count(compiledPredicate);
            return Task.FromResult(result);
        }

        public Task AddAsync(UserRole entity)
        {
            var key = GetCompositeKey(entity);
            _entities.TryAdd(key, entity);
            return Task.CompletedTask;
        }

        public Task AddRangeAsync(IEnumerable<UserRole> entities)
        {
            foreach (var entity in entities)
            {
                var key = GetCompositeKey(entity);
                _entities.TryAdd(key, entity);
            }
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserRole entity)
        {
            var key = GetCompositeKey(entity);
            _entities.AddOrUpdate(key, entity, (k, oldValue) => entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(UserRole entity)
        {
            var key = GetCompositeKey(entity);
            _entities.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            // For UserRole, we can't delete by single Id
            // This operation is not supported for composite key entities
            throw new NotSupportedException("UserRole uses composite key. Use DeleteAsync(UserRole entity) instead.");
        }

        public Task DeleteRangeAsync(IEnumerable<UserRole> entities)
        {
            foreach (var entity in entities)
            {
                var key = GetCompositeKey(entity);
                _entities.TryRemove(key, out _);
            }
            return Task.CompletedTask;
        }
    }
}

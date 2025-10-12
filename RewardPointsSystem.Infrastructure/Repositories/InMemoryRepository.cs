using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Infrastructure.Repositories
{
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly ConcurrentDictionary<Guid, T> _entities;
        private readonly PropertyInfo _idProperty;

        public InMemoryRepository()
        {
            _entities = new ConcurrentDictionary<Guid, T>();
            _idProperty = typeof(T).GetProperty("Id") ?? throw new InvalidOperationException($"Entity {typeof(T).Name} must have an Id property");
        }

        private Guid GetEntityId(T entity)
        {
            return (Guid)_idProperty.GetValue(entity);
        }

        private void SetEntityId(T entity, Guid id)
        {
            _idProperty.SetValue(entity, id);
        }

        public Task<T> GetByIdAsync(Guid id)
        {
            _entities.TryGetValue(id, out var entity);
            return Task.FromResult(entity);
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(_entities.Values.AsEnumerable());
        }

        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            var result = _entities.Values.Where(compiledPredicate);
            return Task.FromResult(result);
        }

        public Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            var result = _entities.Values.SingleOrDefault(compiledPredicate);
            return Task.FromResult(result);
        }

        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            var result = _entities.Values.Any(compiledPredicate);
            return Task.FromResult(result);
        }

        public Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return Task.FromResult(_entities.Count);
            }

            var compiledPredicate = predicate.Compile();
            var result = _entities.Values.Count(compiledPredicate);
            return Task.FromResult(result);
        }

        public Task AddAsync(T entity)
        {
            var id = GetEntityId(entity);
            if (id == Guid.Empty)
            {
                id = Guid.NewGuid();
                SetEntityId(entity, id);
            }

            _entities.TryAdd(id, entity);
            return Task.CompletedTask;
        }

        public Task AddRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                var id = GetEntityId(entity);
                if (id == Guid.Empty)
                {
                    id = Guid.NewGuid();
                    SetEntityId(entity, id);
                }
                _entities.TryAdd(id, entity);
            }
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity)
        {
            var id = GetEntityId(entity);
            _entities.AddOrUpdate(id, entity, (key, oldValue) => entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            var id = GetEntityId(entity);
            _entities.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _entities.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                var id = GetEntityId(entity);
                _entities.TryRemove(id, out _);
            }
            return Task.CompletedTask;
        }
    }
}
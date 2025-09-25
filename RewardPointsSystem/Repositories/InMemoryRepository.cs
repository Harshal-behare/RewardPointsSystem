using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem.Repositories
{
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        protected readonly List<T> _entities = new();
        protected readonly object _lockObject = new();

        public T GetById(Guid id)
        {
            lock (_lockObject)
            {
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty == null)
                    throw new InvalidOperationException($"Entity type {typeof(T).Name} does not have an Id property");

                return _entities.FirstOrDefault(e => 
                    idProperty.GetValue(e) is Guid entityId && entityId == id);
            }
        }

        public IEnumerable<T> GetAll()
        {
            lock (_lockObject)
            {
                return _entities.ToList();
            }
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            lock (_lockObject)
            {
                return _entities.Where(predicate.Compile()).ToList();
            }
        }

        public T SingleOrDefault(Expression<Func<T, bool>> predicate)
        {
            lock (_lockObject)
            {
                return _entities.SingleOrDefault(predicate.Compile());
            }
        }

        public void Add(T entity)
        {
            lock (_lockObject)
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _entities.Add(entity);
            }
        }

        public void AddRange(IEnumerable<T> entities)
        {
            lock (_lockObject)
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                _entities.AddRange(entities);
            }
        }

        public void Remove(T entity)
        {
            lock (_lockObject)
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _entities.Remove(entity);
            }
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            lock (_lockObject)
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                foreach (var entity in entities.ToList())
                {
                    _entities.Remove(entity);
                }
            }
        }

        public void Update(T entity)
        {
            lock (_lockObject)
            {
                // In an in-memory repository, update is automatic
                // since we're working with object references
                // This method is here for interface compatibility
                // In a real database implementation, this would persist changes
            }
        }

        public int Count()
        {
            lock (_lockObject)
            {
                return _entities.Count;
            }
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            lock (_lockObject)
            {
                return _entities.Any(predicate.Compile());
            }
        }
    }
}
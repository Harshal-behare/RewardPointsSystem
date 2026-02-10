using System;

namespace RewardPointsSystem.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a database concurrency conflict occurs.
    /// This is a domain-level abstraction for infrastructure concurrency errors.
    /// </summary>
    public class ConcurrencyException : DomainException
    {
        public ConcurrencyException(string message) 
            : base(message)
        {
        }

        public ConcurrencyException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}

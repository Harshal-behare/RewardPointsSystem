using System;

namespace RewardPointsSystem.Domain.Exceptions
{
    /// <summary>
    /// Base exception for all domain-specific exceptions.
    /// Represents violations of business rules or domain invariants.
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }

        protected DomainException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}

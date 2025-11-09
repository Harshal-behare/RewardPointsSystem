using System;

namespace RewardPointsSystem.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a user points account is not found.
    /// </summary>
    public class UserPointsAccountNotFoundException : DomainException
    {
        public Guid UserId { get; }

        public UserPointsAccountNotFoundException(Guid userId) 
            : base($"User points account for user '{userId}' was not found.")
        {
            UserId = userId;
        }
    }

    /// <summary>
    /// Exception thrown when user has insufficient user points balance.
    /// </summary>
    public class InsufficientUserPointsBalanceException : DomainException
    {
        public Guid UserId { get; }
        public int RequiredPoints { get; }
        public int AvailablePoints { get; }

        public InsufficientUserPointsBalanceException(Guid userId, int requiredUserPoints, int availableUserPoints) 
            : base($"User '{userId}' has insufficient user points balance. Required: {requiredUserPoints}, Available: {availableUserPoints}")
        {
            UserId = userId;
            RequiredPoints = requiredUserPoints;
            AvailablePoints = availableUserPoints;
        }
    }

    /// <summary>
    /// Exception thrown when attempting an invalid user points operation.
    /// </summary>
    public class InvalidUserPointsOperationException : DomainException
    {
        public InvalidUserPointsOperationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a redemption is not found.
    /// </summary>
    public class RedemptionNotFoundException : DomainException
    {
        public Guid RedemptionId { get; }

        public RedemptionNotFoundException(Guid redemptionId) 
            : base($"Redemption with ID '{redemptionId}' was not found.")
        {
            RedemptionId = redemptionId;
        }
    }

    /// <summary>
    /// Exception thrown when attempting an invalid redemption state transition.
    /// </summary>
    public class InvalidRedemptionStateException : DomainException
    {
        public Guid RedemptionId { get; }

        public InvalidRedemptionStateException(Guid redemptionId, string message) 
            : base($"Invalid state transition for redemption '{redemptionId}': {message}")
        {
            RedemptionId = redemptionId;
        }
    }

    /// <summary>
    /// Exception thrown when redemption processing fails.
    /// </summary>
    public class RedemptionProcessingException : DomainException
    {
        public RedemptionProcessingException(string message) : base(message)
        {
        }

        public RedemptionProcessingException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}

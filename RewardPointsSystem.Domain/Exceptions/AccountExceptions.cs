using System;

namespace RewardPointsSystem.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a points account is not found.
    /// </summary>
    public class PointsAccountNotFoundException : DomainException
    {
        public Guid UserId { get; }

        public PointsAccountNotFoundException(Guid userId) 
            : base($"Points account for user '{userId}' was not found.")
        {
            UserId = userId;
        }
    }

    /// <summary>
    /// Exception thrown when user has insufficient points balance.
    /// </summary>
    public class InsufficientPointsBalanceException : DomainException
    {
        public Guid UserId { get; }
        public int RequiredPoints { get; }
        public int AvailablePoints { get; }

        public InsufficientPointsBalanceException(Guid userId, int requiredPoints, int availablePoints) 
            : base($"User '{userId}' has insufficient points balance. Required: {requiredPoints}, Available: {availablePoints}")
        {
            UserId = userId;
            RequiredPoints = requiredPoints;
            AvailablePoints = availablePoints;
        }
    }

    /// <summary>
    /// Exception thrown when attempting an invalid points operation.
    /// </summary>
    public class InvalidPointsOperationException : DomainException
    {
        public InvalidPointsOperationException(string message) : base(message)
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

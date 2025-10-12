using System;

namespace RewardPointsSystem.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when an event is not found.
    /// </summary>
    public class EventNotFoundException : DomainException
    {
        public Guid EventId { get; }

        public EventNotFoundException(Guid eventId) 
            : base($"Event with ID '{eventId}' was not found.")
        {
            EventId = eventId;
        }
    }

    /// <summary>
    /// Exception thrown when event data is invalid.
    /// </summary>
    public class InvalidEventDataException : DomainException
    {
        public InvalidEventDataException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when attempting to perform an invalid event state transition.
    /// </summary>
    public class InvalidEventStateException : DomainException
    {
        public Guid EventId { get; }

        public InvalidEventStateException(Guid eventId, string message) 
            : base($"Invalid state transition for event '{eventId}': {message}")
        {
            EventId = eventId;
        }
    }

    /// <summary>
    /// Exception thrown when event points pool is insufficient.
    /// </summary>
    public class InsufficientEventPointsPoolException : DomainException
    {
        public Guid EventId { get; }
        public int RequestedPoints { get; }
        public int AvailablePoints { get; }

        public InsufficientEventPointsPoolException(Guid eventId, int requestedPoints, int availablePoints) 
            : base($"Event '{eventId}' has insufficient points pool. Requested: {requestedPoints}, Available: {availablePoints}")
        {
            EventId = eventId;
            RequestedPoints = requestedPoints;
            AvailablePoints = availablePoints;
        }
    }

    /// <summary>
    /// Exception thrown when user is not registered for an event.
    /// </summary>
    public class UserNotRegisteredForEventException : DomainException
    {
        public Guid UserId { get; }
        public Guid EventId { get; }

        public UserNotRegisteredForEventException(Guid userId, Guid eventId) 
            : base($"User '{userId}' is not registered for event '{eventId}'.")
        {
            UserId = userId;
            EventId = eventId;
        }
    }

    /// <summary>
    /// Exception thrown when user is already registered for an event.
    /// </summary>
    public class UserAlreadyRegisteredForEventException : DomainException
    {
        public Guid UserId { get; }
        public Guid EventId { get; }

        public UserAlreadyRegisteredForEventException(Guid userId, Guid eventId) 
            : base($"User '{userId}' is already registered for event '{eventId}'.")
        {
            UserId = userId;
            EventId = eventId;
        }
    }

    /// <summary>
    /// Exception thrown when user has already been awarded points for an event.
    /// </summary>
    public class DuplicatePointsAwardException : DomainException
    {
        public Guid UserId { get; }
        public Guid EventId { get; }

        public DuplicatePointsAwardException(Guid userId, Guid eventId) 
            : base($"User '{userId}' has already been awarded points for event '{eventId}'.")
        {
            UserId = userId;
            EventId = eventId;
        }
    }
}

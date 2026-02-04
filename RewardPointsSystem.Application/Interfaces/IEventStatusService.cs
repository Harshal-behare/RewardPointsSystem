using RewardPointsSystem.Application.DTOs.Events;

namespace RewardPointsSystem.Application.Interfaces;

/// <summary>
/// Application layer service for event status management.
/// Handles status transition logic that was previously in the controller.
/// </summary>
public interface IEventStatusService
{
    /// <summary>
    /// Changes the event status with proper validation and workflow logic.
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <param name="targetStatus">The target status to change to</param>
    /// <returns>Result of the operation</returns>
    Task<EventStatusChangeResult> ChangeStatusAsync(Guid eventId, string targetStatus);
}

/// <summary>
/// Result of an event status change operation
/// </summary>
public class EventStatusChangeResult
{
    public bool Success { get; set; }
    public EventResponseDto? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public EventStatusErrorType ErrorType { get; set; }

    public static EventStatusChangeResult Succeeded(EventResponseDto data) => 
        new() { Success = true, Data = data };
    
    public static EventStatusChangeResult Failed(string message, EventStatusErrorType errorType = EventStatusErrorType.ValidationError) => 
        new() { Success = false, ErrorMessage = message, ErrorType = errorType };
}

public enum EventStatusErrorType
{
    None,
    NotFound,
    ValidationError,
    InvalidTransition
}

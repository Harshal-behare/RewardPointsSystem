using RewardPointsSystem.Application.DTOs.Events;

namespace RewardPointsSystem.Application.Interfaces;

/// <summary>
/// Application layer service for querying event participants.
/// Handles DTO mapping that was previously in the controller.
/// </summary>
public interface IEventParticipantQueryService
{
    /// <summary>
    /// Gets participants for an event with proper DTO mapping.
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <returns>List of participant DTOs, or null if event not found</returns>
    Task<IEnumerable<EventParticipantResponseDto>?> GetEventParticipantsAsync(Guid eventId);
}

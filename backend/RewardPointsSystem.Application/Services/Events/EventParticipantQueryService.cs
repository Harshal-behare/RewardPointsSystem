using RewardPointsSystem.Application.DTOs.Events;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.Services.Events;

/// <summary>
/// Application layer service for querying event participants.
/// Encapsulates DTO mapping that was previously in the controller.
/// </summary>
public class EventParticipantQueryService : IEventParticipantQueryService
{
    private readonly IEventService _eventService;
    private readonly IEventParticipationService _participationService;

    public EventParticipantQueryService(
        IEventService eventService,
        IEventParticipationService participationService)
    {
        _eventService = eventService;
        _participationService = participationService;
    }

    public async Task<IEnumerable<EventParticipantResponseDto>?> GetEventParticipantsAsync(Guid eventId)
    {
        // Verify event exists
        var eventEntity = await _eventService.GetEventByIdAsync(eventId);
        if (eventEntity == null)
        {
            return null;
        }

        var participants = await _participationService.GetEventParticipantsAsync(eventId);
        
        return participants.Select(p => new EventParticipantResponseDto
        {
            Id = p.Id,
            EventId = p.EventId,
            UserId = p.UserId,
            UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}" : "Unknown",
            UserEmail = p.User?.Email ?? "Unknown",
            RegisteredAt = p.RegisteredAt,
            Status = p.AttendanceStatus.ToString(),
            PointsAwarded = p.PointsAwarded,
            EventRank = p.EventRank
        });
    }
}

using RewardPointsSystem.Application.DTOs.Events;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Events;

namespace RewardPointsSystem.Application.Services.Events;

/// <summary>
/// Application layer service for event status management.
/// Encapsulates status transition business logic that was previously in the controller.
/// </summary>
public class EventStatusService : IEventStatusService
{
    private readonly IEventService _eventService;
    private readonly IEventQueryService _eventQueryService;

    public EventStatusService(
        IEventService eventService,
        IEventQueryService eventQueryService)
    {
        _eventService = eventService;
        _eventQueryService = eventQueryService;
    }

    public async Task<EventStatusChangeResult> ChangeStatusAsync(Guid eventId, string targetStatus)
    {
        var existingEvent = await _eventService.GetEventByIdAsync(eventId);
        if (existingEvent == null)
        {
            return EventStatusChangeResult.Failed($"Event with ID {eventId} not found", EventStatusErrorType.NotFound);
        }

        try
        {
            // Apply the status change based on the target status
            // 4 valid statuses: Draft, Upcoming, Active, Completed
            switch (targetStatus?.ToLower())
            {
                case "draft":
                    // Can only revert to draft from Upcoming
                    if (existingEvent.Status == EventStatus.Upcoming)
                    {
                        await _eventService.RevertToDraftAsync(eventId);
                    }
                    else if (existingEvent.Status != EventStatus.Draft)
                    {
                        return EventStatusChangeResult.Failed(
                            $"Cannot change to Draft from {existingEvent.Status} status", 
                            EventStatusErrorType.InvalidTransition);
                    }
                    break;
                    
                case "upcoming":
                case "published":
                    await _eventService.PublishEventAsync(eventId);
                    break;
                    
                case "active":
                    await _eventService.ActivateEventAsync(eventId);
                    break;
                    
                case "completed":
                    await _eventService.CompleteEventAsync(eventId);
                    break;
                    
                default:
                    return EventStatusChangeResult.Failed(
                        $"Invalid status: {targetStatus}. Valid values are: Draft, Upcoming, Active, Completed",
                        EventStatusErrorType.ValidationError);
            }

            // Get the updated event with proper DTO mapping
            var eventDto = await _eventQueryService.GetEventByIdAsync(eventId);
            return EventStatusChangeResult.Succeeded(eventDto!);
        }
        catch (InvalidOperationException ex)
        {
            return EventStatusChangeResult.Failed(ex.Message, EventStatusErrorType.InvalidTransition);
        }
    }
}

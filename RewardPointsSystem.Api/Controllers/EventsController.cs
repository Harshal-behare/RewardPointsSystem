using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Events;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Events;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages event-related operations
    /// </summary>
    public class EventsController : BaseApiController
    {
        private readonly IEventService _eventService;
        private readonly IEventParticipationService _participationService;
        private readonly IUserService _userService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(
            IEventService eventService,
            IEventParticipationService participationService,
            IUserService userService,
            ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _participationService = participationService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Maps backend EventStatus to frontend-friendly status string
        /// 4 statuses: Draft, Upcoming, Active, Completed
        /// </summary>
        private string MapEventStatusToFrontend(EventStatus status)
        {
            return status switch
            {
                EventStatus.Draft => "Draft",
                EventStatus.Upcoming => "Upcoming",
                EventStatus.Active => "Active",
                EventStatus.Completed => "Completed",
                _ => "Draft"
            };
        }

        /// <summary>
        /// Maps an Event entity to EventResponseDto
        /// </summary>
        private EventResponseDto MapToEventResponseDto(Event e, Dictionary<Guid, string> userNames = null)
        {
            userNames ??= new Dictionary<Guid, string>();

            var winners = e.Participants?
                .Where(p => p.PointsAwarded.HasValue && p.EventRank.HasValue)
                .OrderBy(p => p.EventRank)
                .Take(3)
                .Select(p => new EventWinnerDto
                {
                    UserId = p.UserId,
                    UserName = userNames.TryGetValue(p.UserId, out var name) ? name : "Unknown",
                    Rank = p.EventRank!.Value,
                    PointsAwarded = p.PointsAwarded!.Value
                })
                .ToList() ?? new List<EventWinnerDto>();

            return new EventResponseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                EventEndDate = e.EventEndDate,
                Status = MapEventStatusToFrontend(e.Status),
                TotalPointsPool = e.TotalPointsPool,
                RemainingPoints = e.GetAvailablePointsPool(),
                ParticipantsCount = e.Participants?.Count ?? 0,
                MaxParticipants = e.MaxParticipants,
                RegistrationStartDate = e.RegistrationStartDate,
                RegistrationEndDate = e.RegistrationEndDate,
                Location = e.Location,
                VirtualLink = e.VirtualLink,
                BannerImageUrl = e.BannerImageUrl,
                CreatedAt = e.CreatedAt,
                FirstPlacePoints = e.FirstPlacePoints,
                SecondPlacePoints = e.SecondPlacePoints,
                ThirdPlacePoints = e.ThirdPlacePoints,
                Winners = winners
            };
        }

        /// <summary>
        /// Get all visible events for employees (Upcoming, Active, Completed - excludes Draft)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllEvents()
        {
            try
            {
                // Get events visible to employees using service (handles auto-status updates)
                var events = await _eventService.GetVisibleEventsAsync();
                var eventsList = events.ToList();

                // Load user names for winners
                var userNames = await GetUserNamesForEventsAsync(eventsList);

                var eventDtos = eventsList.Select(e => MapToEventResponseDto(e, userNames));

                return Success(eventDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events");
                return Error("Failed to retrieve events");
            }
        }

        /// <summary>
        /// Helper to load user names for event participants/winners
        /// </summary>
        private async Task<Dictionary<Guid, string>> GetUserNamesForEventsAsync(IEnumerable<Event> events)
        {
            // Get all unique user IDs from participants with awards (winners)
            var userIds = events
                .SelectMany(e => e.Participants ?? Enumerable.Empty<EventParticipant>())
                .Where(p => p.PointsAwarded.HasValue && p.EventRank.HasValue)
                .Select(p => p.UserId)
                .Distinct()
                .ToList();

            if (!userIds.Any())
                return new Dictionary<Guid, string>();

            // Load all users and create lookup
            var allUsers = await _userService.GetActiveUsersAsync();
            return allUsers
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");
        }

        /// <summary>
        /// Get event by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<EventResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            try
            {
                // Use EventService (handles auto-status updates)
                var ev = await _eventService.GetEventByIdAsync(id);

                if (ev == null)
                    return NotFoundError($"Event with ID {id} not found");

                // Load user names for winners
                var userNames = await GetUserNamesForEventsAsync(new[] { ev });

                var eventDto = MapToEventResponseDto(ev, userNames);

                return Success(eventDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event {EventId}", id);
                return Error("Failed to retrieve event");
            }
        }

        /// <summary>
        /// Create a new event (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<EventResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            try
            {
                var ev = await _eventService.CreateEventAsync(
                    dto.Name, 
                    dto.Description, 
                    dto.EventDate, 
                    dto.TotalPointsPool,
                    dto.MaxParticipants,
                    dto.RegistrationStartDate,
                    dto.RegistrationEndDate,
                    dto.Location,
                    dto.VirtualLink,
                    dto.BannerImageUrl,
                    dto.EventEndDate,
                    dto.FirstPlacePoints,
                    dto.SecondPlacePoints,
                    dto.ThirdPlacePoints);

                var eventDto = MapToEventResponseDto(ev);

                return Created(eventDto, "Event created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                return Error("Failed to create event");
            }
        }

        /// <summary>
        /// Update an existing event (Admin only)
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="dto">Event update data</param>
        /// <response code="200">Event updated successfully</response>
        /// <response code="404">Event not found</response>
        /// <response code="422">Validation failed</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<EventResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto dto)
        {
            try
            {
                var existingEvent = await _eventService.GetEventByIdAsync(id);
                if (existingEvent == null)
                    return NotFoundError($"Event with ID {id} not found");

                var ev = await _eventService.UpdateEventAsync(id, dto);

                var eventDto = MapToEventResponseDto(ev);

                return Success(eventDto, "Event updated successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Event with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event {EventId}", id);
                return Error("Failed to update event");
            }
        }

        /// <summary>
        /// Cancel/Delete an event (Admin only)
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <response code="200">Event cancelled successfully</response>
        /// <response code="404">Event not found</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            try
            {
                var existingEvent = await _eventService.GetEventByIdAsync(id);
                if (existingEvent == null)
                    return NotFoundError($"Event with ID {id} not found");

                await _eventService.DeleteEventAsync(id);

                return Success<object>(null, "Event cancelled successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Event with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling event {EventId}", id);
                return Error("Failed to cancel event");
            }
        }

        /// <summary>
        /// Change event status (Admin only)
        /// Valid statuses: Draft, Upcoming, Active, Completed
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="dto">Status change data</param>
        /// <response code="200">Event status updated successfully</response>
        /// <response code="404">Event not found</response>
        /// <response code="400">Invalid status transition</response>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<EventResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeEventStatus(Guid id, [FromBody] ChangeEventStatusDto dto)
        {
            try
            {
                var existingEvent = await _eventService.GetEventByIdAsync(id);
                if (existingEvent == null)
                    return NotFoundError($"Event with ID {id} not found");

                // Apply the status change based on the target status
                // 4 valid statuses: Draft, Upcoming, Active, Completed
                switch (dto.Status?.ToLower())
                {
                    case "draft":
                        // Can only revert to draft from Upcoming
                        if (existingEvent.Status == EventStatus.Upcoming)
                        {
                            await _eventService.RevertToDraftAsync(id);
                        }
                        else if (existingEvent.Status != EventStatus.Draft)
                        {
                            return Error($"Cannot change to Draft from {existingEvent.Status} status", 400);
                        }
                        break;
                    case "upcoming":
                    case "published":
                        await _eventService.PublishEventAsync(id);
                        break;
                    case "active":
                        await _eventService.ActivateEventAsync(id);
                        break;
                    case "completed":
                        await _eventService.CompleteEventAsync(id);
                        break;
                    default:
                        return Error($"Invalid status: {dto.Status}. Valid values are: Draft, Upcoming, Active, Completed", 400);
                }

                var ev = await _eventService.GetEventByIdAsync(id);

                var eventDto = MapToEventResponseDto(ev);

                return Success(eventDto, $"Event status changed to {dto.Status}");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Event with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing event status {EventId}", id);
                return Error("Failed to change event status");
            }
        }

        /// <summary>
        /// Get participants for a specific event
        /// </summary>
        /// <param name="id">Event ID from route</param>
        /// <response code="200">Returns list of participants</response>
        /// <response code="404">Event not found</response>
        [HttpGet("{id}/participants")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventParticipantResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventParticipants(Guid id)
        {
            try
            {
                var eventEntity = await _eventService.GetEventByIdAsync(id);
                if (eventEntity == null)
                {
                    return NotFoundError($"Event with ID {id} not found");
                }

                var participants = await _participationService.GetEventParticipantsAsync(id);
                var participantDtos = participants.Select(p => new EventParticipantResponseDto
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

                return Success(participantDtos);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Event with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving participants for event {EventId}", id);
                return Error("Failed to retrieve event participants");
            }
        }

        /// <summary>
        /// Register participant for an event
        /// </summary>
        /// <param name="id">Event ID from route</param>
        /// <param name="dto">Participant registration data</param>
        /// <response code="200">Participant registered successfully</response>
        /// <response code="404">Event or user not found</response>
        [HttpPost("{id}/participants")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegisterParticipant(Guid id, [FromBody] RegisterParticipantDto dto)
        {
            try
            {
                // Use the route 'id' parameter instead of dto.EventId for consistency
                await _participationService.RegisterParticipantAsync(id, dto.UserId);
                return Success<object>(null, "Participant registered successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Event or user not found");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering participant for event {EventId}", id);
                return Error("Failed to register participant");
            }
        }

        /// <summary>
        /// Get all events including past events (Admin only)
        /// </summary>
        /// <response code="200">Returns all events</response>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllEventsAdmin()
        {
            try
            {
                var events = await _eventService.GetAllEventsAsync();
                var eventDtos = events.Select(e => MapToEventResponseDto(e));

                return Success(eventDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all events");
                return Error("Failed to retrieve events");
            }
        }
    }
}

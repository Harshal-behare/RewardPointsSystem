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
        private readonly ILogger<EventsController> _logger;

        public EventsController(
            IEventService eventService,
            IEventParticipationService participationService,
            ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _participationService = participationService;
            _logger = logger;
        }

        /// <summary>
        /// Maps backend EventStatus to frontend-friendly status string
        /// </summary>
        private string MapEventStatusToFrontend(EventStatus status)
        {
            return status switch
            {
                EventStatus.Draft => "Draft",
                EventStatus.Published or EventStatus.Upcoming => "Upcoming",
                EventStatus.RegistrationOpen => "Upcoming",
                EventStatus.RegistrationClosed => "Upcoming",
                EventStatus.InProgress or EventStatus.Active => "Active",
                EventStatus.Completed => "Completed",
                EventStatus.Cancelled => "Cancelled",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Get all events
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllEvents()
        {
            try
            {
                var events = await _eventService.GetUpcomingEventsAsync();
                var eventDtos = events.Select(e => new EventResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Status = MapEventStatusToFrontend(e.Status),
                    TotalPointsPool = e.TotalPointsPool,
                    RemainingPoints = e.GetAvailablePointsPool(),
                    CreatedAt = e.CreatedAt
                });

                return Success(eventDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events");
                return Error("Failed to retrieve events");
            }
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
                var ev = await _eventService.GetEventByIdAsync(id);
                if (ev == null)
                    return NotFoundError($"Event with ID {id} not found");

                var eventDto = new EventResponseDto
                {
                    Id = ev.Id,
                    Name = ev.Name,
                    Description = ev.Description,
                    EventDate = ev.EventDate,
                    Status = MapEventStatusToFrontend(ev.Status),
                    TotalPointsPool = ev.TotalPointsPool,
                    RemainingPoints = ev.GetAvailablePointsPool(),
                    CreatedAt = ev.CreatedAt
                };

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
                var ev = await _eventService.CreateEventAsync(dto.Name, dto.Description, dto.EventDate, dto.TotalPointsPool);

                var eventDto = new EventResponseDto
                {
                    Id = ev.Id,
                    Name = ev.Name,
                    Description = ev.Description,
                    EventDate = ev.EventDate,
                    Status = MapEventStatusToFrontend(ev.Status),
                    TotalPointsPool = ev.TotalPointsPool,
                    RemainingPoints = ev.GetAvailablePointsPool(),
                    CreatedAt = ev.CreatedAt
                };

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

                var eventDto = new EventResponseDto
                {
                    Id = ev.Id,
                    Name = ev.Name,
                    Description = ev.Description,
                    EventDate = ev.EventDate,
                    Status = MapEventStatusToFrontend(ev.Status),
                    TotalPointsPool = ev.TotalPointsPool,
                    RemainingPoints = ev.GetAvailablePointsPool(),
                    ParticipantsCount = ev.Participants?.Count ?? 0,
                    CreatedAt = ev.CreatedAt
                };

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

                await _eventService.CancelEventAsync(id);

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
                // Accept both backend and frontend status names
                switch (dto.Status?.ToLower())
                {
                    case "published":
                    case "upcoming":
                        await _eventService.PublishEventAsync(id);
                        break;
                    case "active":
                    case "inprogress":
                        await _eventService.ActivateEventAsync(id);
                        break;
                    case "completed":
                        await _eventService.CompleteEventAsync(id);
                        break;
                    case "cancelled":
                        await _eventService.CancelEventAsync(id);
                        break;
                    default:
                        return Error($"Invalid status: {dto.Status}. Valid values are: Upcoming, Active, Completed, Cancelled", 400);
                }

                var ev = await _eventService.GetEventByIdAsync(id);

                var eventDto = new EventResponseDto
                {
                    Id = ev.Id,
                    Name = ev.Name,
                    Description = ev.Description,
                    EventDate = ev.EventDate,
                    Status = MapEventStatusToFrontend(ev.Status),
                    TotalPointsPool = ev.TotalPointsPool,
                    RemainingPoints = ev.GetAvailablePointsPool(),
                    ParticipantsCount = ev.Participants?.Count ?? 0,
                    CreatedAt = ev.CreatedAt
                };

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
                var eventDtos = events.Select(e => new EventResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Status = MapEventStatusToFrontend(e.Status),
                    TotalPointsPool = e.TotalPointsPool,
                    RemainingPoints = e.GetAvailablePointsPool(),
                    ParticipantsCount = e.Participants?.Count ?? 0,
                    CreatedAt = e.CreatedAt
                });

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

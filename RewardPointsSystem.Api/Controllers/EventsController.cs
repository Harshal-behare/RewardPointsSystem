using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Events;
using RewardPointsSystem.Application.Interfaces;

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
                    Status = e.Status.ToString(),
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
                    Status = ev.Status.ToString(),
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
                    Status = ev.Status.ToString(),
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
        /// Register participant for an event
        /// </summary>
        [HttpPost("{id}/participants")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegisterParticipant(Guid id, [FromBody] RegisterParticipantDto dto)
        {
            try
            {
                await _participationService.RegisterParticipantAsync(dto.EventId, dto.UserId);
                return Success<object>(null, "Participant registered successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("Event or user not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering participant");
                return Error("Failed to register participant");
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Events;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages event-related operations.
    /// Clean Architecture compliant - delegates business logic to Application layer services.
    /// </summary>
    public class EventsController : BaseApiController
    {
        private readonly IEventService _eventService;
        private readonly IEventQueryService _eventQueryService;
        private readonly IEventParticipationService _participationService;
        private readonly IPointsAwardingService _pointsAwardingService;
        private readonly IEventStatusService _eventStatusService;
        private readonly IEventParticipantQueryService _participantQueryService;

        public EventsController(
            IEventService eventService,
            IEventQueryService eventQueryService,
            IEventParticipationService participationService,
            IPointsAwardingService pointsAwardingService,
            IEventStatusService eventStatusService,
            IEventParticipantQueryService participantQueryService)
        {
            _eventService = eventService;
            _eventQueryService = eventQueryService;
            _participationService = participationService;
            _pointsAwardingService = pointsAwardingService;
            _eventStatusService = eventStatusService;
            _participantQueryService = participantQueryService;
        }

        /// <summary>
        /// Get all visible events for employees (Upcoming, Active, Completed - excludes Draft)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EventResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllEvents()
        {
            var eventDtos = await _eventQueryService.GetVisibleEventsAsync();
            return Success(eventDtos);
        }

        /// <summary>
        /// Get event by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<EventResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventById(Guid id)
        {
            var eventDto = await _eventQueryService.GetEventByIdAsync(id);
            if (eventDto == null)
                return NotFoundError($"Event with ID {id} not found");

            return Success(eventDto);
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

            // Use query service to get properly mapped DTO
            var eventDto = await _eventQueryService.GetEventByIdAsync(ev.Id);
            return Created(eventDto!, "Event created successfully");
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
            var existingEvent = await _eventService.GetEventByIdAsync(id);
            if (existingEvent == null)
                return NotFoundError($"Event with ID {id} not found");

            await _eventService.UpdateEventAsync(id, dto);

            var eventDto = await _eventQueryService.GetEventByIdAsync(id);
            return Success(eventDto!, "Event updated successfully");
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
            var existingEvent = await _eventService.GetEventByIdAsync(id);
            if (existingEvent == null)
                return NotFoundError($"Event with ID {id} not found");

            await _eventService.DeleteEventAsync(id);

            return Success<object>(null, "Event cancelled successfully");
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
            var result = await _eventStatusService.ChangeStatusAsync(id, dto.Status!);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    EventStatusErrorType.NotFound => NotFoundError(result.ErrorMessage!),
                    EventStatusErrorType.InvalidTransition => Error(result.ErrorMessage!, 400),
                    EventStatusErrorType.ValidationError => Error(result.ErrorMessage!, 400),
                    _ => Error(result.ErrorMessage!, 400)
                };
            }

            return Success(result.Data!, $"Event status changed to {dto.Status}");
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
            var participantDtos = await _participantQueryService.GetEventParticipantsAsync(id);

            if (participantDtos == null)
                return NotFoundError($"Event with ID {id} not found");

            return Success(participantDtos);
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
            var eventDtos = await _eventQueryService.GetAllEventsAsync();
            return Success(eventDtos);
        }

        /// <summary>
        /// Bulk award points to event winners (Admin only)
        /// Awards points to multiple winners in a single transaction
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="dto">List of winners with points and ranks</param>
        /// <response code="200">Points awarded successfully</response>
        /// <response code="404">Event not found</response>
        /// <response code="400">Validation failed (not enough points, already awarded, etc.)</response>
        [HttpPost("{id}/award-winners")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AwardWinners(Guid id, [FromBody] BulkAwardWinnersDto dto)
        {
            try
            {
                var eventEntity = await _eventService.GetEventByIdAsync(id);
                if (eventEntity == null)
                    return NotFoundError($"Event with ID {id} not found");

                // Validate awards using service layer
                var validation = await _pointsAwardingService.ValidateWinnerAwardsAsync(id, dto.Awards);
                if (!validation.IsValid)
                    return Error(validation.ErrorMessage!, 400);

                // Get the current admin's user ID for budget tracking
                var adminId = GetCurrentUserId();
                await _pointsAwardingService.BulkAwardPointsAsync(id, dto.Awards, adminId);

                var eventDto = await _eventQueryService.GetEventByIdAsync(id);
                return Success(eventDto!, "Points awarded to winners successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, 400);
            }
        }

        /// <summary>
        /// Unregister a participant from an event
        /// Self-unregister or admin can unregister anyone
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="userId">User ID to unregister</param>
        /// <response code="200">Participant unregistered successfully</response>
        /// <response code="404">Event or participant not found</response>
        /// <response code="400">Cannot unregister (event started, already awarded, etc.)</response>
        /// <response code="403">Not authorized to unregister this user</response>
        [HttpDelete("{id}/participants/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UnregisterParticipant(Guid id, Guid userId)
        {
            try
            {
                // Check authorization - either self or admin
                var currentUserId = GetCurrentUserId();
                var isAdmin = User.IsInRole("Admin");

                if (currentUserId != userId && !isAdmin)
                    return StatusCode(403, new ErrorResponse { Message = "Not authorized to unregister this participant" });

                // Validate unregistration using service layer
                var validation = await _participationService.ValidateUnregisterAsync(id, userId);
                if (!validation.CanUnregister)
                {
                    if (validation.ErrorMessage?.Contains("not found") == true || 
                        validation.ErrorMessage?.Contains("not registered") == true)
                        return NotFoundError(validation.ErrorMessage);
                    return Error(validation.ErrorMessage!, 400);
                }

                await _participationService.RemoveParticipantAsync(id, userId);

                return Success<object>(null, "Participant unregistered successfully");
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("not registered"))
                    return NotFoundError("User is not registered for this event");
                return Error(ex.Message, 400);
            }
        }

        /// <summary>
        /// Get active event registrations count for a specific user (Admin only - for deactivation check)
        /// Returns count of events where user is registered and status is Upcoming or Active,
        /// plus count of completed events where user has pending point awards
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <response code="200">Returns count of active event registrations and pending awards</response>
        [HttpGet("user/{userId}/active-registrations-count")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<UserEventRegistrationsCountDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserActiveEventRegistrationsCount(Guid userId)
        {
            var (activeRegistrations, pendingAwards) = await _eventQueryService.GetUserActiveRegistrationsCountAsync(userId);

            return Success(new UserEventRegistrationsCountDto 
            { 
                Count = activeRegistrations, 
                PendingAwardsCount = pendingAwards 
            });
        }
    }
}

using RewardPointsSystem.Application.DTOs.Events;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Service interface for event query operations.
    /// Handles event retrieval and DTO mapping - Clean Architecture compliant.
    /// </summary>
    public interface IEventQueryService
    {
        /// <summary>
        /// Get all visible events for employees (excludes Draft status).
        /// Automatically updates event statuses based on dates.
        /// </summary>
        /// <returns>Collection of event response DTOs</returns>
        Task<IEnumerable<EventResponseDto>> GetVisibleEventsAsync();

        /// <summary>
        /// Get all events including drafts (Admin view).
        /// </summary>
        /// <returns>Collection of all event response DTOs</returns>
        Task<IEnumerable<EventResponseDto>> GetAllEventsAsync();

        /// <summary>
        /// Get event by ID with full details.
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <returns>Event response DTO or null</returns>
        Task<EventResponseDto?> GetEventByIdAsync(Guid eventId);

        /// <summary>
        /// Get events registered by a specific user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Collection of event response DTOs</returns>
        Task<IEnumerable<EventResponseDto>> GetUserRegisteredEventsAsync(Guid userId);

        /// <summary>
        /// Get active registrations count for a user (for deactivation checks).
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Count of active registrations and pending awards</returns>
        Task<(int ActiveRegistrations, int PendingAwards)> GetUserActiveRegistrationsCountAsync(Guid userId);

        /// <summary>
        /// Check if user is registered for an event.
        /// </summary>
        /// <param name="eventId">Event ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>True if registered</returns>
        Task<bool> IsUserRegisteredAsync(Guid eventId, Guid userId);
    }
}

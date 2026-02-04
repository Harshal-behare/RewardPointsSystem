using Microsoft.Extensions.Logging;
using RewardPointsSystem.Application.DTOs.Events;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Events;

namespace RewardPointsSystem.Application.Services.Events
{
    /// <summary>
    /// Service implementation for event query operations.
    /// Handles all event retrieval and DTO mapping - Clean Architecture compliant.
    /// </summary>
    public class EventQueryService : IEventQueryService
    {
        private readonly IEventService _eventService;
        private readonly IEventParticipationService _participationService;
        private readonly IUserService _userService;
        private readonly IPointsAwardingService _pointsAwardingService;
        private readonly ILogger<EventQueryService> _logger;

        public EventQueryService(
            IEventService eventService,
            IEventParticipationService participationService,
            IUserService userService,
            IPointsAwardingService pointsAwardingService,
            ILogger<EventQueryService> logger)
        {
            _eventService = eventService;
            _participationService = participationService;
            _userService = userService;
            _pointsAwardingService = pointsAwardingService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EventResponseDto>> GetVisibleEventsAsync()
        {
            var events = await _eventService.GetVisibleEventsAsync();
            var eventsList = events.ToList();
            var userNames = await GetUserNamesForEventsAsync(eventsList);
            return eventsList.Select(e => MapToEventResponseDto(e, userNames));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EventResponseDto>> GetAllEventsAsync()
        {
            var events = await _eventService.GetAllEventsAsync();
            var eventsList = events.ToList();
            var userNames = await GetUserNamesForEventsAsync(eventsList);
            return eventsList.Select(e => MapToEventResponseDto(e, userNames));
        }

        /// <inheritdoc />
        public async Task<EventResponseDto?> GetEventByIdAsync(Guid eventId)
        {
            var ev = await _eventService.GetEventByIdAsync(eventId);
            if (ev == null)
                return null;

            var userNames = await GetUserNamesForEventsAsync(new[] { ev });
            return MapToEventResponseDto(ev, userNames);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<EventResponseDto>> GetUserRegisteredEventsAsync(Guid userId)
        {
            var allEvents = await _eventService.GetVisibleEventsAsync();
            var userEvents = new List<Event>();

            foreach (var ev in allEvents)
            {
                var isRegistered = ev.Participants?.Any(p => p.UserId == userId) ?? false;
                if (isRegistered)
                {
                    userEvents.Add(ev);
                }
            }

            var userNames = await GetUserNamesForEventsAsync(userEvents);
            return userEvents.Select(e => MapToEventResponseDto(e, userNames));
        }

        /// <inheritdoc />
        public async Task<(int ActiveRegistrations, int PendingAwards)> GetUserActiveRegistrationsCountAsync(Guid userId)
        {
            var allEvents = await _eventService.GetAllEventsAsync();
            
            // Count active registrations (Upcoming or Active status)
            var activeRegistrations = allEvents.Count(e =>
                (e.Status == EventStatus.Upcoming || e.Status == EventStatus.Active) &&
                (e.Participants?.Any(p => p.UserId == userId) ?? false));

            // Count pending awards (Completed events where user is registered but hasn't been awarded)
            var pendingAwards = allEvents.Count(e =>
                e.Status == EventStatus.Completed &&
                (e.Participants?.Any(p => p.UserId == userId && !p.PointsAwarded.HasValue) ?? false));

            return (activeRegistrations, pendingAwards);
        }

        /// <inheritdoc />
        public async Task<bool> IsUserRegisteredAsync(Guid eventId, Guid userId)
        {
            return await _participationService.IsUserRegisteredAsync(eventId, userId);
        }

        #region Private Mapping Methods

        /// <summary>
        /// Maps backend EventStatus to frontend-friendly status string.
        /// </summary>
        private static string MapEventStatusToFrontend(EventStatus status)
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
        /// Maps an Event entity to EventResponseDto.
        /// </summary>
        private static EventResponseDto MapToEventResponseDto(Event e, Dictionary<Guid, string>? userNames = null)
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
        /// Loads user names for event participants/winners.
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

        #endregion
    }
}

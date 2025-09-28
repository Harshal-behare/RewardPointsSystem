using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Events;

namespace RewardPointsSystem.Interfaces
{
    public interface IEventParticipationService
    {
        Task RegisterParticipantAsync(Guid eventId, Guid userId);
        Task<IEnumerable<EventParticipant>> GetEventParticipantsAsync(Guid eventId);
        Task<IEnumerable<EventParticipant>> GetUserEventsAsync(Guid userId);
        Task RemoveParticipantAsync(Guid eventId, Guid userId);
        Task<bool> IsUserRegisteredAsync(Guid eventId, Guid userId);
    }
}
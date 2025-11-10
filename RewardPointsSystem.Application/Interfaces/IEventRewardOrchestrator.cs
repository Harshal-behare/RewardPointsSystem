using System;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Accounts;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IEventRewardOrchestrator
    /// Responsibility: Coordinate event reward flow only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IEventRewardOrchestrator
    {
        Task<EventRewardResult> ProcessEventRewardAsync(Guid eventId, Guid userId, int points, int position, Guid awardedBy);
    }

    /// <summary>
    /// Result DTO for Event Reward Processing
    /// </summary>
    public class EventRewardResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string EventName { get; set; }
        public EventParticipant Participation { get; set; }
        public UserPointsTransaction Transaction { get; set; }
    }
}
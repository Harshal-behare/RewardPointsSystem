using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Events;
using RewardPointsSystem.Models.Accounts;

namespace RewardPointsSystem.Interfaces
{
    public interface IEventRewardOrchestrator
    {
        Task<EventParticipationResult> ProcessEventParticipationAsync(Guid eventId, Guid userId);
        Task<IEnumerable<PointsTransaction>> ProcessBulkEventParticipationAsync(Guid eventId, IEnumerable<Guid> userIds);
        Task<EventRewardSummary> GetEventRewardSummaryAsync(Guid eventId);
        Task<bool> ValidateEventEligibilityAsync(Guid eventId, Guid userId);
    }

    public class EventParticipationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public EventParticipant Participation { get; set; }
        public PointsTransaction Transaction { get; set; }
    }

    public class EventRewardSummary
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public int TotalParticipants { get; set; }
        public decimal TotalPointsAwarded { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
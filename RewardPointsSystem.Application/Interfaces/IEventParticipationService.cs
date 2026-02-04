using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Events;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IEventParticipationService
    /// Responsibility: Manage event participants only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IEventParticipationService
    {
        Task RegisterParticipantAsync(Guid eventId, Guid userId);
        Task<IEnumerable<EventParticipant>> GetEventParticipantsAsync(Guid eventId);
        Task<IEnumerable<EventParticipant>> GetUserEventsAsync(Guid userId);
        Task RemoveParticipantAsync(Guid eventId, Guid userId);
        Task<bool> IsUserRegisteredAsync(Guid eventId, Guid userId);
        
        /// <summary>
        /// Validate if a participant can be unregistered from an event.
        /// Checks event status and whether points have been awarded.
        /// </summary>
        Task<UnregisterValidationResult> ValidateUnregisterAsync(Guid eventId, Guid userId);
    }

    /// <summary>
    /// Result of unregister validation
    /// </summary>
    public class UnregisterValidationResult
    {
        public bool CanUnregister { get; set; }
        public string? ErrorMessage { get; set; }

        public static UnregisterValidationResult Success() => new() { CanUnregister = true };
        public static UnregisterValidationResult Failed(string message) => new() { CanUnregister = false, ErrorMessage = message };
    }
}
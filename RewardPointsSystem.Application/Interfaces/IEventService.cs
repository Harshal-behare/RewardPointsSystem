using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IEventService
    /// Responsibility: Manage event lifecycle only
    /// Status flow: Draft → Upcoming → Active → Completed
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IEventService
    {
        Task<Event> CreateEventAsync(string name, string description, DateTime date, int pointsPool);
        Task<Event> UpdateEventAsync(Guid id, UpdateEventDto updates);
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event> GetEventByIdAsync(Guid id);
        
        /// <summary>
        /// Publish event: Draft → Upcoming (makes event visible to employees)
        /// </summary>
        Task PublishEventAsync(Guid id);
        
        /// <summary>
        /// Activate event: Upcoming → Active (event is currently in progress)
        /// </summary>
        Task ActivateEventAsync(Guid id);
        
        /// <summary>
        /// Complete event: Upcoming or Active → Completed (event is finished, award points)
        /// </summary>
        Task CompleteEventAsync(Guid id);
        
        /// <summary>
        /// Revert to draft: Upcoming → Draft (hide event from employees)
        /// </summary>
        Task RevertToDraftAsync(Guid id);
        
        Task DeleteEventAsync(Guid id);
    }
}
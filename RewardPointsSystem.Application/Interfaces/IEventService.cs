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
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IEventService
    {
        Task<Event> CreateEventAsync(string name, string description, DateTime date, int pointsPool);
        Task<Event> UpdateEventAsync(Guid id, UpdateEventDto updates);
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
        Task<IEnumerable<Event>> GetActiveEventsAsync();
        Task<Event> GetEventByIdAsync(Guid id);
        Task PublishEventAsync(Guid id);
        Task ActivateEventAsync(Guid id);
        Task CompleteEventAsync(Guid id);
        Task CancelEventAsync(Guid id);
    }
}
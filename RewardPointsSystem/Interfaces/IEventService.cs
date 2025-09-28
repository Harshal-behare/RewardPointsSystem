using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Events;

namespace RewardPointsSystem.Interfaces
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(string name, string description, DateTime date, int pointsPool);
        Task<Event> UpdateEventAsync(Guid id, EventUpdateDto updates);
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
        Task<IEnumerable<Event>> GetActiveEventsAsync();
        Task<Event> GetEventByIdAsync(Guid id);
        Task CompleteEventAsync(Guid id);
        Task CancelEventAsync(Guid id);
    }
    
    public class EventUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? EventDate { get; set; }
        public int? TotalPointsPool { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RewardPointsSystem.Models;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem.Services
{
    public class EventService : IEventService
    {
        private readonly List<Event> _events = new();

        public void CreateEvent(Event evnt) => _events.Add(evnt);

        public IEnumerable<Event> GetAllEvents() => _events;
    }
}


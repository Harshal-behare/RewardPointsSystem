using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RewardPointsSystem.Interfaces
{
    public interface IPointsAwardingService
    {
        Task AwardPointsAsync(Guid eventId, Guid userId, int points, int position);
        Task BulkAwardPointsAsync(Guid eventId, List<WinnerDto> winners);
        Task<bool> HasUserBeenAwardedAsync(Guid eventId, Guid userId);
        Task<int> GetRemainingPointsPoolAsync(Guid eventId);
    }

    public class WinnerDto
    {
        public Guid UserId { get; set; }
        public int Points { get; set; }
        public int Position { get; set; }
    }
}
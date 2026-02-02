using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IPointsAwardingService
    /// Responsibility: Award points to event winners only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IPointsAwardingService
    {
        Task AwardPointsAsync(Guid eventId, Guid userId, int points, int position, Guid? awardingAdminId = null);
        Task AwardPointsAsync(Guid userId, int points, string description, Guid? eventId = null, Guid? awardingAdminId = null);
        Task BulkAwardPointsAsync(Guid eventId, List<WinnerDto> winners, Guid? awardingAdminId = null);
        Task<bool> HasUserBeenAwardedAsync(Guid eventId, Guid userId);
        Task<int> GetRemainingPointsPoolAsync(Guid eventId);
    }
}
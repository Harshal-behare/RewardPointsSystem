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
        
        /// <summary>
        /// Bulk award points to event winners with validation.
        /// Validates ranks are unique, within 1-3, and match event configuration if set.
        /// </summary>
        Task BulkAwardPointsAsync(Guid eventId, List<WinnerDto> winners, Guid? awardingAdminId = null);
        
        /// <summary>
        /// Validate winner awards before processing.
        /// Returns validation result with error message if invalid.
        /// </summary>
        Task<AwardValidationResult> ValidateWinnerAwardsAsync(Guid eventId, List<WinnerDto> winners);
        
        Task<bool> HasUserBeenAwardedAsync(Guid eventId, Guid userId);
        Task<int> GetRemainingPointsPoolAsync(Guid eventId);
    }

    /// <summary>
    /// Result of award validation
    /// </summary>
    public class AwardValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public static AwardValidationResult Success() => new() { IsValid = true };
        public static AwardValidationResult Failed(string message) => new() { IsValid = false, ErrorMessage = message };
    }
}
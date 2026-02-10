using System;
using System.Threading.Tasks;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Service interface for points management operations (credit/debit).
    /// Handles all points manipulation with validation - Clean Architecture compliant.
    /// </summary>
    public interface IPointsManagementService
    {
        /// <summary>
        /// Deduct points from a user's account with validation.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="points">Points to deduct</param>
        /// <param name="reason">Reason for deduction</param>
        /// <param name="adminUserId">Admin performing the deduction</param>
        /// <returns>Result with success/failure and error message</returns>
        Task<PointsOperationResult> DeductPointsAsync(Guid userId, int points, string reason, Guid adminUserId);

        /// <summary>
        /// Validate if points can be deducted from a user's account.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="points">Points to validate</param>
        /// <returns>Validation result</returns>
        Task<PointsOperationResult> ValidateDeductionAsync(Guid userId, int points);
    }

    /// <summary>
    /// Result of points operation
    /// </summary>
    public class PointsOperationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int? CurrentBalance { get; set; }

        public static PointsOperationResult Succeeded(int? currentBalance = null) 
            => new() { Success = true, CurrentBalance = currentBalance };
        public static PointsOperationResult Failed(string message, int? currentBalance = null) 
            => new() { Success = false, ErrorMessage = message, CurrentBalance = currentBalance };
    }
}

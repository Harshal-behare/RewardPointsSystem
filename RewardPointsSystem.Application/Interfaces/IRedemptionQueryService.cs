using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Redemptions;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IRedemptionQueryService
    /// Responsibility: Query redemption data (read operations)
    /// Clean Architecture - Separates read/write concerns
    /// </summary>
    public interface IRedemptionQueryService
    {
        /// <summary>
        /// Get all redemptions for admin view (with user and product details)
        /// </summary>
        Task<IEnumerable<RedemptionResponseDto>> GetAllRedemptionsAsync();
        
        /// <summary>
        /// Get redemptions for a specific user
        /// </summary>
        Task<IEnumerable<RedemptionResponseDto>> GetUserRedemptionsAsync(Guid userId);
        
        /// <summary>
        /// Get redemption details by ID
        /// </summary>
        Task<RedemptionDetailsDto?> GetRedemptionByIdAsync(Guid redemptionId);
        
        /// <summary>
        /// Get redemptions filtered by status
        /// </summary>
        Task<IEnumerable<RedemptionResponseDto>> GetRedemptionsByStatusAsync(string status);

        /// <summary>
        /// Get all redemptions with pagination (admin sees all, employee sees own)
        /// </summary>
        Task<(IEnumerable<RedemptionResponseDto> Redemptions, int TotalCount)> GetRedemptionsPagedAsync(
            Guid? userId, bool isAdmin, string? statusFilter, int page, int pageSize);

        /// <summary>
        /// Get user's redemptions with pagination
        /// </summary>
        Task<(IEnumerable<RedemptionResponseDto> Redemptions, int TotalCount)> GetUserRedemptionsPagedAsync(
            Guid userId, int page, int pageSize);

        /// <summary>
        /// Get pending redemptions (admin only)
        /// </summary>
        Task<IEnumerable<RedemptionResponseDto>> GetPendingRedemptionsAsync();

        /// <summary>
        /// Get all redemptions history with pagination (admin only)
        /// </summary>
        Task<(IEnumerable<RedemptionResponseDto> Redemptions, int TotalCount)> GetRedemptionHistoryPagedAsync(
            int page, int pageSize);

        /// <summary>
        /// Get pending/approved redemptions count for a user
        /// </summary>
        Task<int> GetUserPendingRedemptionsCountAsync(Guid userId);

        /// <summary>
        /// Get pending redemptions count for a product
        /// </summary>
        Task<int> GetProductPendingRedemptionsCountAsync(Guid productId);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Points;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Query service for points-related data retrieval with enriched DTOs.
    /// Handles complex queries that require joining multiple data sources.
    /// </summary>
    public interface IPointsQueryService
    {
        /// <summary>
        /// Get user's points account with user details
        /// </summary>
        Task<PointsAccountResponseDto?> GetUserPointsAccountAsync(Guid userId);

        /// <summary>
        /// Get user's transaction history with event/redemption details (paginated)
        /// </summary>
        Task<(IEnumerable<TransactionResponseDto> Transactions, int TotalCount)> GetUserTransactionsAsync(
            Guid userId, int page, int pageSize);

        /// <summary>
        /// Get all transactions for admin view (paginated)
        /// </summary>
        Task<(IEnumerable<TransactionResponseDto> Transactions, int TotalCount)> GetAllTransactionsAsync(
            int page, int pageSize);

        /// <summary>
        /// Get points leaderboard with user details
        /// </summary>
        Task<IEnumerable<PointsAccountResponseDto>> GetLeaderboardAsync(int top);

        /// <summary>
        /// Get points system summary statistics
        /// </summary>
        Task<PointsSummaryDto> GetPointsSummaryAsync();
    }
}

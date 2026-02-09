using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Points;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Accounts;

namespace RewardPointsSystem.Application.Services.Points
{
    /// <summary>
    /// Query service for points-related data retrieval with enriched DTOs.
    /// </summary>
    public class PointsQueryService : IPointsQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserPointsAccountService _accountService;
        private readonly IUserPointsTransactionService _transactionService;

        public PointsQueryService(
            IUnitOfWork unitOfWork,
            IUserPointsAccountService accountService,
            IUserPointsTransactionService transactionService)
        {
            _unitOfWork = unitOfWork;
            _accountService = accountService;
            _transactionService = transactionService;
        }

        public async Task<PointsAccountResponseDto?> GetUserPointsAccountAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return null;

            var account = await _accountService.GetAccountAsync(userId);
            if (account == null)
            {
                // Auto-create account if it doesn't exist
                account = await _accountService.CreateAccountAsync(userId);
            }

            return new PointsAccountResponseDto
            {
                UserId = account.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                UserEmail = user.Email,
                CurrentBalance = account.CurrentBalance,
                TotalEarned = account.TotalEarned,
                TotalRedeemed = account.TotalRedeemed,
                PendingPoints = account.PendingPoints,
                LastTransaction = account.LastUpdatedAt,
                CreatedAt = account.CreatedAt
            };
        }

        public async Task<(IEnumerable<TransactionResponseDto> Transactions, int TotalCount)> GetUserTransactionsAsync(
            Guid userId, int page, int pageSize)
        {
            var transactions = await _transactionService.GetUserTransactionsAsync(userId);
            var transactionList = transactions.ToList();

            // Get events and event participants for enriching transaction data
            var allEvents = await _unitOfWork.Events.GetAllAsync();
            var allEventParticipants = await _unitOfWork.EventParticipants.GetAllAsync();
            var userParticipants = allEventParticipants.Where(ep => ep.UserId == userId).ToList();

            var totalCount = transactionList.Count;
            var pagedTransactions = transactionList
                .OrderByDescending(t => t.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => MapToTransactionDto(t, allEvents, userParticipants))
                .ToList();

            return (pagedTransactions, totalCount);
        }

        public async Task<(IEnumerable<TransactionResponseDto> Transactions, int TotalCount)> GetAllTransactionsAsync(
            int page, int pageSize)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var transactionList = transactions.ToList();

            var totalCount = transactionList.Count;
            var pagedTransactions = transactionList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionResponseDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    TransactionType = t.TransactionType.ToString(),
                    UserPoints = t.UserPoints,
                    Description = t.Description,
                    EventId = t.TransactionSource == TransactionOrigin.Event ? t.SourceId : null,
                    RedemptionId = t.TransactionSource == TransactionOrigin.Redemption ? t.SourceId : null,
                    TransactionSource = t.TransactionSource.ToString(),
                    BalanceAfter = t.BalanceAfter,
                    Timestamp = t.Timestamp
                })
                .ToList();

            return (pagedTransactions, totalCount);
        }

        public async Task<IEnumerable<PointsAccountResponseDto>> GetLeaderboardAsync(int top)
        {
            var accounts = await _accountService.GetTopAccountsAsync(top);
            var leaderboard = new List<PointsAccountResponseDto>();

            foreach (var account in accounts)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(account.UserId);
                leaderboard.Add(new PointsAccountResponseDto
                {
                    UserId = account.UserId,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    UserEmail = user?.Email,
                    CurrentBalance = account.CurrentBalance,
                    TotalEarned = account.TotalEarned,
                    TotalRedeemed = account.TotalRedeemed,
                    LastTransaction = account.LastUpdatedAt,
                    CreatedAt = account.CreatedAt
                });
            }

            return leaderboard;
        }

        public async Task<PointsSummaryDto> GetPointsSummaryAsync()
        {
            var allAccounts = await _accountService.GetAllAccountsAsync();
            var accountList = allAccounts.ToList();

            return new PointsSummaryDto
            {
                TotalUsers = accountList.Count,
                TotalPointsDistributed = accountList.Sum(a => a.TotalEarned),
                TotalPointsRedeemed = accountList.Sum(a => a.TotalRedeemed),
                TotalPointsInCirculation = accountList.Sum(a => a.CurrentBalance),
                AverageBalance = accountList.Any() ? accountList.Average(a => a.CurrentBalance) : 0
            };
        }

        private TransactionResponseDto MapToTransactionDto(
            UserPointsTransaction t,
            IEnumerable<Domain.Entities.Events.Event> allEvents,
            IEnumerable<Domain.Entities.Events.EventParticipant> userParticipants)
        {
            var isEventTransaction = t.TransactionSource == TransactionOrigin.Event;
            var eventId = isEventTransaction ? t.SourceId : (Guid?)null;
            string eventName = null;
            string eventDescription = null;
            int? eventRank = null;

            if (isEventTransaction && eventId.HasValue)
            {
                var evt = allEvents.FirstOrDefault(e => e.Id == eventId.Value);
                eventName = evt?.Name;
                eventDescription = evt?.Description;
                var participant = userParticipants.FirstOrDefault(p => p.EventId == eventId.Value);
                eventRank = participant?.EventRank;
            }

            return new TransactionResponseDto
            {
                Id = t.Id,
                UserId = t.UserId,
                TransactionType = t.TransactionType.ToString(),
                UserPoints = t.UserPoints,
                Description = t.Description,
                EventId = eventId,
                EventName = eventName,
                EventDescription = eventDescription,
                EventRank = eventRank,
                RedemptionId = t.TransactionSource == TransactionOrigin.Redemption ? t.SourceId : null,
                TransactionSource = t.TransactionSource.ToString(),
                BalanceAfter = t.BalanceAfter,
                Timestamp = t.Timestamp
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Services.Admin
{
    /// <summary>
    /// Service: AdminDashboardService
    /// Responsibility: Provide admin queries only (read-only aggregations)
    /// </summary>
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryService _inventoryService;

        public AdminDashboardService(IUnitOfWork unitOfWork, IInventoryService inventoryService)
        {
            _unitOfWork = unitOfWork;
            _inventoryService = inventoryService;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            // Use CountAsync with predicates instead of loading entire tables
            var totalUsers = await _unitOfWork.Users.CountAsync();
            var activeUsers = await _unitOfWork.Users.CountAsync(u => u.IsActive);
            var totalEvents = await _unitOfWork.Events.CountAsync();
            var activeEvents = await _unitOfWork.Events.CountAsync(e => e.Status == EventStatus.Upcoming);
            var activeProducts = await _unitOfWork.Products.CountAsync(p => p.IsActive);
            var pendingRedemptions = await _unitOfWork.Redemptions.CountAsync(r => r.Status == RedemptionStatus.Pending);
            var totalRedemptions = await _unitOfWork.Redemptions.CountAsync();

            // For aggregations, we still need to query but can be more selective
            var accounts = await _unitOfWork.UserPointsAccounts.GetAllAsync();
            var totalPointsDistributed = accounts.Sum(a => a.TotalEarned);
            var totalPointsRedeemed = accounts.Sum(a => a.TotalRedeemed);

            return new DashboardStats
            {
                TotalUsers = totalUsers,
                TotalActiveUsers = activeUsers,
                TotalEvents = totalEvents,
                ActiveEvents = activeEvents,
                TotalProducts = activeProducts,
                ActiveProducts = activeProducts,
                TotalPointsDistributed = totalPointsDistributed,
                TotalPointsRedeemed = totalPointsRedeemed,
                PendingRedemptions = pendingRedemptions,
                TotalRedemptions = totalRedemptions,
                TotalPointsAwarded = totalPointsDistributed,
                UserEventParticipation = new Dictionary<string, int>(), // Loaded on demand
                UserPointsEarned = new Dictionary<string, int>() // Loaded on demand
            };
        }

        public async Task<IEnumerable<Event>> GetEventsNeedingAllocationAsync()
        {
            // Use FindAsync with predicate instead of GetAllAsync
            var completedEvents = await _unitOfWork.Events.FindAsync(e => e.Status == EventStatus.Completed);
            var participants = await _unitOfWork.EventParticipants.FindAsync(p => p.PointsAwarded == null);

            // Events that are completed but have participants without points awarded
            var participantEventIds = participants.Select(p => p.EventId).ToHashSet();
            var eventsNeedingAllocation = completedEvents
                .Where(e => participantEventIds.Contains(e.Id))
                .ToList();

            return eventsNeedingAllocation;
        }

        public async Task<IEnumerable<Redemption>> GetPendingRedemptionsAsync()
        {
            // Use FindAsync with predicate instead of GetAllAsync
            return await _unitOfWork.Redemptions.FindAsync(r => r.Status == RedemptionStatus.Pending);
        }

        public async Task<IEnumerable<InventoryAlert>> GetInventoryAlertsAsync()
        {
            return await _inventoryService.GetLowStockItemsAsync();
        }

        public async Task<PointsSummary> GetPointsSummaryAsync()
        {
            var events = await _unitOfWork.Events.GetAllAsync();
            var participants = await _unitOfWork.EventParticipants.GetAllAsync();
            var accounts = await _unitOfWork.UserPointsAccounts.GetAllAsync();
            var transactions = await _unitOfWork.UserPointsTransactions.GetAllAsync();
            var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
            var products = await _unitOfWork.Products.GetAllAsync();

            var eventParticipationCounts = events.ToDictionary(
                e => e.Id.ToString(),
                e => participants.Count(p => p.EventId == e.Id)
            );

            var eventPointsAwarded = events.ToDictionary(
                e => e.Id.ToString(),
                e => participants.Where(p => p.EventId == e.Id && p.PointsAwarded.HasValue)
                              .Sum(p => p.PointsAwarded.Value)
            );

            var productRedemptionCounts = products.ToDictionary(
                p => p.Id.ToString(),
                p => redemptions.Count(r => r.ProductId == p.Id && r.Status == RedemptionStatus.Approved)
            );

            var productRedemptionValues = products.ToDictionary(
                p => p.Id.ToString(),
                p => redemptions.Where(r => r.ProductId == p.Id && r.Status == RedemptionStatus.Approved)
                               .Sum(r => r.PointsSpent)
            );

            var inventory = await _unitOfWork.Inventory.GetAllAsync();
            var currentStock = products.ToDictionary(
                p => p.Id.ToString(),
                p => inventory.FirstOrDefault(i => i.ProductId == p.Id)?.QuantityAvailable ?? 0
            );

            var lowStockProducts = (await _inventoryService.GetLowStockItemsAsync())
                .Where(alert => alert.AlertType == "LOW_STOCK")
                .Select(alert => products.FirstOrDefault(p => p.Id == alert.ProductId))
                .Where(p => p != null);

            var outOfStockProducts = (await _inventoryService.GetLowStockItemsAsync())
                .Where(alert => alert.AlertType == "OUT_OF_STOCK")
                .Select(alert => products.FirstOrDefault(p => p.Id == alert.ProductId))
                .Where(p => p != null);

            return new PointsSummary
            {
                TotalPointsInCirculation = accounts.Sum(a => a.CurrentBalance),
                TotalPointsAwarded = accounts.Sum(a => a.TotalEarned),
                TotalPointsRedeemed = accounts.Sum(a => a.TotalRedeemed),
                EventParticipationCounts = eventParticipationCounts,
                EventPointsAwarded = eventPointsAwarded,
                ProductRedemptionCounts = productRedemptionCounts,
                ProductRedemptionValues = productRedemptionValues,
                CurrentStock = currentStock,
                LowStockProducts = lowStockProducts,
                OutOfStockProducts = outOfStockProducts
            };
        }
    }
}
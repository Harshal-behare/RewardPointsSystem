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
            var users = await _unitOfWork.Users.GetAllAsync();
            var events = await _unitOfWork.Events.GetAllAsync();
            var participants = await _unitOfWork.EventParticipants.GetAllAsync();
            var accounts = await _unitOfWork.UserPointsAccounts.GetAllAsync();
            var transactions = await _unitOfWork.UserPointsTransactions.GetAllAsync();
            var redemptions = await _unitOfWork.Redemptions.GetAllAsync();

            var activeUsers = users.Where(u => u.IsActive);
            var activeEvents = events.Where(e => e.Status == EventStatus.Active);
            var completedEvents = events.Where(e => e.Status == EventStatus.Completed);

            var userEventParticipation = activeUsers.ToDictionary(
                u => u.Id.ToString(),
                u => participants.Count(p => p.UserId == u.Id)
            );

            var userPointsEarned = activeUsers.ToDictionary(
                u => u.Id.ToString(),
                u => accounts.FirstOrDefault(a => a.UserId == u.Id)?.TotalEarned ?? 0
            );

            return new DashboardStats
            {
                TotalUsers = users.Count(),
                ActiveEvents = activeEvents.Count(),
                PendingRedemptions = redemptions.Count(r => r.Status == RedemptionStatus.Pending),
                TotalPointsAwarded = accounts.Sum(a => a.TotalEarned),
                TotalPointsRedeemed = accounts.Sum(a => a.TotalRedeemed),
                UserEventParticipation = userEventParticipation,
                UserPointsEarned = userPointsEarned
            };
        }

        public async Task<IEnumerable<Event>> GetEventsNeedingAllocationAsync()
        {
            var events = await _unitOfWork.Events.GetAllAsync();
            var participants = await _unitOfWork.EventParticipants.GetAllAsync();

            // Events that are completed but have participants without points awarded
            var eventsNeedingAllocation = events
                .Where(e => e.Status == EventStatus.Completed)
                .Where(e => participants.Any(p => p.EventId == e.Id && p.PointsAwarded == null))
                .ToList();

            return eventsNeedingAllocation;
        }

        public async Task<IEnumerable<Redemption>> GetPendingRedemptionsAsync()
        {
            var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
            return redemptions.Where(r => r.Status == RedemptionStatus.Pending);
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
                p => redemptions.Count(r => r.ProductId == p.Id && r.Status == RedemptionStatus.Delivered)
            );

            var productRedemptionValues = products.ToDictionary(
                p => p.Id.ToString(),
                p => redemptions.Where(r => r.ProductId == p.Id && r.Status == RedemptionStatus.Delivered)
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Domain.Entities.Accounts;

namespace RewardPointsSystem.Application.Services.Employee
{
    /// <summary>
    /// Service: EmployeeDashboardService
    /// Responsibility: Provide employee dashboard data (read-only aggregations)
    /// </summary>
    public class EmployeeDashboardService : IEmployeeDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeDashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<EmployeeDashboardDto> GetDashboardAsync(Guid userId)
        {
            var dashboard = new EmployeeDashboardDto
            {
                MyPoints = await GetMyPointsAsync(userId),
                MyRedemptions = await GetMyRedemptionsAsync(userId),
                AvailableEvents = await GetAvailableEventsAsync(userId),
                RecentActivity = await GetRecentActivityAsync(userId),
                FeaturedProducts = await GetFeaturedProductsAsync()
            };

            return dashboard;
        }

        private async Task<MyPointsDto> GetMyPointsAsync(Guid userId)
        {
            var account = await _unitOfWork.UserPointsAccounts.SingleOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
            {
                return new MyPointsDto
                {
                    CurrentBalance = 0,
                    PendingPoints = 0,
                    TotalEarned = 0,
                    TotalRedeemed = 0
                };
            }

            return new MyPointsDto
            {
                CurrentBalance = account.CurrentBalance,
                PendingPoints = account.PendingPoints,
                TotalEarned = account.TotalEarned,
                TotalRedeemed = account.TotalRedeemed
            };
        }

        private async Task<MyRedemptionsDto> GetMyRedemptionsAsync(Guid userId)
        {
            var redemptions = await _unitOfWork.Redemptions.FindAsync(r => r.UserId == userId);

            return new MyRedemptionsDto
            {
                Pending = redemptions.Count(r => r.Status == RedemptionStatus.Pending),
                Approved = redemptions.Count(r => r.Status == RedemptionStatus.Approved),
                Delivered = redemptions.Count(r => r.Status == RedemptionStatus.Delivered)
            };
        }

        private async Task<AvailableEventsDto> GetAvailableEventsAsync(Guid userId)
        {
            // Get all events visible to employees (not Draft)
            var events = await _unitOfWork.Events.FindAsync(e => e.Status != EventStatus.Draft);

            // Get user's registered events
            var userParticipations = await _unitOfWork.EventParticipants.FindAsync(p => p.UserId == userId);
            var registeredEventIds = userParticipations.Select(p => p.EventId).ToHashSet();

            var upcoming = events
                .Where(e => e.Status == EventStatus.Upcoming)
                .OrderBy(e => e.EventDate)
                .Take(5)
                .Select(e => MapToEventSummary(e, registeredEventIds.Contains(e.Id)))
                .ToList();

            var active = events
                .Where(e => e.Status == EventStatus.Active)
                .OrderBy(e => e.EventDate)
                .Take(5)
                .Select(e => MapToEventSummary(e, registeredEventIds.Contains(e.Id)))
                .ToList();

            var myRegistered = events
                .Where(e => registeredEventIds.Contains(e.Id) && e.Status != EventStatus.Completed)
                .OrderBy(e => e.EventDate)
                .Take(5)
                .Select(e => MapToEventSummary(e, true))
                .ToList();

            return new AvailableEventsDto
            {
                Upcoming = upcoming,
                Active = active,
                MyRegistered = myRegistered
            };
        }

        private EventSummaryDto MapToEventSummary(Event e, bool isRegistered)
        {
            return new EventSummaryDto
            {
                Id = e.Id,
                Name = e.Name,
                EventDate = e.EventDate,
                Status = e.Status.ToString(),
                IsRegistered = isRegistered
            };
        }

        private async Task<List<RecentActivityDto>> GetRecentActivityAsync(Guid userId)
        {
            var activities = new List<RecentActivityDto>();

            // Get recent transactions
            var transactions = await _unitOfWork.UserPointsTransactions.FindAsync(t => t.UserId == userId);
            var recentTransactions = transactions
                .OrderByDescending(t => t.Timestamp)
                .Take(10);

            foreach (var t in recentTransactions)
            {
                activities.Add(new RecentActivityDto
                {
                    Type = MapTransactionCategory(t.TransactionType),
                    Description = t.Description,
                    Points = t.UserPoints,
                    Timestamp = t.Timestamp
                });
            }

            // Get recent event registrations
            var participations = await _unitOfWork.EventParticipants.FindAsync(p => p.UserId == userId);
            var recentParticipations = participations
                .OrderByDescending(p => p.RegisteredAt)
                .Take(5);

            var eventIds = recentParticipations.Select(p => p.EventId).ToList();
            var events = await _unitOfWork.Events.FindAsync(e => eventIds.Contains(e.Id));
            var eventDict = events.ToDictionary(e => e.Id, e => e.Name);

            foreach (var p in recentParticipations)
            {
                var eventName = eventDict.ContainsKey(p.EventId) ? eventDict[p.EventId] : "Unknown Event";
                activities.Add(new RecentActivityDto
                {
                    Type = "registered",
                    Description = $"Registered for {eventName}",
                    Points = null,
                    Timestamp = p.RegisteredAt
                });
            }

            // Sort by timestamp and return top 10
            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();
        }

        private string MapTransactionCategory(TransactionCategory category)
        {
            return category switch
            {
                TransactionCategory.Earned => "earned",
                TransactionCategory.Redeemed => "redeemed",
                _ => "other"
            };
        }

        private async Task<List<FeaturedProductDto>> GetFeaturedProductsAsync()
        {
            // Get active products with their pricing and inventory
            var products = await _unitOfWork.Products.FindAsync(p => p.IsActive);
            var pricings = await _unitOfWork.Pricing.GetAllAsync();
            var inventory = await _unitOfWork.Inventory.GetAllAsync();

            var pricingDict = pricings
                .Where(p => p.IsActive)
                .GroupBy(p => p.ProductId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.EffectiveFrom).First());

            var inventoryDict = inventory.ToDictionary(i => i.ProductId, i => i);

            // Get products sorted by lowest points cost (affordable first)
            var featuredProducts = products
                .Where(p => pricingDict.ContainsKey(p.Id))
                .OrderBy(p => pricingDict[p.Id].PointsCost)
                .Take(6)
                .Select(p => new FeaturedProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    PointsCost = pricingDict.ContainsKey(p.Id) ? pricingDict[p.Id].PointsCost : 0,
                    ImageUrl = p.ImageUrl,
                    IsInStock = inventoryDict.ContainsKey(p.Id) && inventoryDict[p.Id].QuantityAvailable > 0
                })
                .ToList();

            return featuredProducts;
        }
    }
}

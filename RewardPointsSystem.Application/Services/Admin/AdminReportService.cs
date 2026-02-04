using System;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Admin;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Operations;

namespace RewardPointsSystem.Application.Services.Admin
{
    /// <summary>
    /// Service: AdminReportService
    /// Responsibility: Generate admin reports with proper aggregations
    /// Clean Architecture - Application layer encapsulates all business logic
    /// </summary>
    public class AdminReportService : IAdminReportService
    {
        private readonly IUserPointsAccountService _accountService;
        private readonly IUserService _userService;
        private readonly IEventService _eventService;
        private readonly IUnitOfWork _unitOfWork;

        public AdminReportService(
            IUserPointsAccountService accountService,
            IUserService userService,
            IEventService eventService,
            IUnitOfWork unitOfWork)
        {
            _accountService = accountService;
            _userService = userService;
            _eventService = eventService;
            _unitOfWork = unitOfWork;
        }

        public async Task<PointsReportDto> GetPointsReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var effectiveStartDate = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var effectiveEndDate = endDate ?? DateTime.UtcNow;

            var accounts = await _accountService.GetAllAccountsAsync();
            var accountsList = accounts.ToList();

            return new PointsReportDto
            {
                Period = new ReportPeriodDto 
                { 
                    Start = effectiveStartDate, 
                    End = effectiveEndDate 
                },
                TotalUsers = accountsList.Count,
                TotalPointsDistributed = accountsList.Sum(a => a.TotalEarned),
                TotalPointsRedeemed = accountsList.Sum(a => a.TotalRedeemed),
                TotalPointsInCirculation = accountsList.Sum(a => a.CurrentBalance),
                AverageBalance = accountsList.Any() ? accountsList.Average(a => a.CurrentBalance) : 0,
                TopEarners = accountsList
                    .OrderByDescending(a => a.TotalEarned)
                    .Take(10)
                    .Select(a => new TopEarnerDto
                    {
                        UserId = a.UserId,
                        TotalEarned = a.TotalEarned,
                        CurrentBalance = a.CurrentBalance
                    })
            };
        }

        public async Task<UsersReportDto> GetUsersReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var effectiveStartDate = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var effectiveEndDate = endDate ?? DateTime.UtcNow;

            var users = await _userService.GetActiveUsersAsync();
            var usersList = users.ToList();

            return new UsersReportDto
            {
                Period = new ReportPeriodDto 
                { 
                    Start = effectiveStartDate, 
                    End = effectiveEndDate 
                },
                TotalUsers = usersList.Count,
                ActiveUsers = usersList.Count(u => u.IsActive),
                NewUsersInPeriod = usersList.Count(u => 
                    u.CreatedAt >= effectiveStartDate && u.CreatedAt <= effectiveEndDate)
            };
        }

        public async Task<RedemptionsReportDto> GetRedemptionsReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var effectiveStartDate = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var effectiveEndDate = endDate ?? DateTime.UtcNow;

            var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
            var periodRedemptions = redemptions
                .Where(r => r.RequestedAt >= effectiveStartDate && r.RequestedAt <= effectiveEndDate)
                .ToList();

            return new RedemptionsReportDto
            {
                Period = new ReportPeriodDto 
                { 
                    Start = effectiveStartDate, 
                    End = effectiveEndDate 
                },
                TotalRedemptions = periodRedemptions.Count,
                PendingRedemptions = periodRedemptions.Count(r => r.Status == RedemptionStatus.Pending),
                ApprovedRedemptions = periodRedemptions.Count(r => r.Status == RedemptionStatus.Approved),
                CancelledRedemptions = periodRedemptions.Count(r => r.Status == RedemptionStatus.Cancelled),
                TotalPointsSpent = periodRedemptions.Sum(r => r.PointsSpent)
            };
        }

        public async Task<EventsReportDto> GetEventsReportAsync(int? year)
        {
            var effectiveYear = year ?? DateTime.UtcNow.Year;

            var events = await _eventService.GetAllEventsAsync();
            var yearEvents = events.Where(e => e.EventDate.Year == effectiveYear).ToList();

            return new EventsReportDto
            {
                Year = effectiveYear,
                TotalEvents = yearEvents.Count,
                CompletedEvents = yearEvents.Count(e => e.Status == EventStatus.Completed),
                UpcomingEvents = yearEvents.Count(e => e.Status == EventStatus.Upcoming),
                TotalParticipants = yearEvents.Sum(e => e.Participants.Count),
                TotalPointsDistributed = yearEvents.Sum(e => e.TotalPointsPool - e.GetAvailablePointsPool())
            };
        }

        public async Task<AdminCountDto> GetAdminCountAsync()
        {
            var adminRole = await _unitOfWork.Roles.GetAllAsync();
            var admin = adminRole.FirstOrDefault(r => r.Name.Equals("admin", StringComparison.OrdinalIgnoreCase));

            if (admin == null)
            {
                return new AdminCountDto { Count = 0 };
            }

            var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
            var adminUserIds = userRoles
                .Where(ur => ur.RoleId == admin.Id)
                .Select(ur => ur.UserId)
                .ToList();

            var users = await _userService.GetActiveUsersAsync();
            var activeAdminCount = users.Count(u => adminUserIds.Contains(u.Id) && u.IsActive);

            return new AdminCountDto { Count = activeAdminCount };
        }
    }
}

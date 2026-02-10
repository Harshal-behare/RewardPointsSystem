using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Redemptions;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Operations;

namespace RewardPointsSystem.Application.Services.Redemptions
{
    /// <summary>
    /// Service: RedemptionQueryService
    /// Responsibility: Query redemption data (read operations)
    /// Clean Architecture Compliant - Encapsulates data access logic
    /// </summary>
    public class RedemptionQueryService : IRedemptionQueryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RedemptionQueryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IEnumerable<RedemptionResponseDto>> GetAllRedemptionsAsync()
        {
            var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
            var users = await _unitOfWork.Users.GetAllAsync();
            var products = await _unitOfWork.Products.GetAllAsync();

            return redemptions.Select(r => MapToResponseDto(r, users, products))
                              .OrderByDescending(r => r.RequestedAt);
        }

        public async Task<IEnumerable<RedemptionResponseDto>> GetUserRedemptionsAsync(Guid userId)
        {
            var redemptions = await _unitOfWork.Redemptions.FindAsync(r => r.UserId == userId);
            var users = await _unitOfWork.Users.GetAllAsync();
            var products = await _unitOfWork.Products.GetAllAsync();

            return redemptions.Select(r => MapToResponseDto(r, users, products))
                              .OrderByDescending(r => r.RequestedAt);
        }

        public async Task<RedemptionDetailsDto?> GetRedemptionByIdAsync(Guid redemptionId)
        {
            var redemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionId);
            if (redemption == null)
                return null;

            var user = await _unitOfWork.Users.GetByIdAsync(redemption.UserId);
            var product = await _unitOfWork.Products.GetByIdAsync(redemption.ProductId);
            
            string? approverName = null;
            if (redemption.ApprovedBy.HasValue)
            {
                var approver = await _unitOfWork.Users.GetByIdAsync(redemption.ApprovedBy.Value);
                approverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : null;
            }

            // Get product category if product exists
            string? categoryName = null;
            if (product?.CategoryId != null)
            {
                var category = await _unitOfWork.ProductCategories.GetByIdAsync(product.CategoryId.Value);
                categoryName = category?.Name;
            }

            return new RedemptionDetailsDto
            {
                Id = redemption.Id,
                UserId = redemption.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                UserEmail = user?.Email ?? "",
                ProductId = redemption.ProductId,
                ProductName = product?.Name ?? "Unknown Product",
                ProductCategory = categoryName ?? "",
                PointsSpent = redemption.PointsSpent,
                Status = redemption.Status.ToString(),
                RequestedAt = redemption.RequestedAt,
                ApprovedAt = redemption.ApprovedAt,
                ApprovedBy = redemption.ApprovedBy,
                ApprovedByName = approverName,
                ProcessedAt = redemption.ProcessedAt,
                RejectionReason = redemption.RejectionReason ?? ""
            };
        }

        public async Task<IEnumerable<RedemptionResponseDto>> GetRedemptionsByStatusAsync(string status)
        {
            var allRedemptions = await GetAllRedemptionsAsync();
            
            if (Enum.TryParse<RedemptionStatus>(status, true, out var statusEnum))
            {
                return allRedemptions.Where(r => r.Status.Equals(statusEnum.ToString(), StringComparison.OrdinalIgnoreCase));
            }
            
            return allRedemptions;
        }

        private static RedemptionResponseDto MapToResponseDto(
            Redemption redemption,
            IEnumerable<Domain.Entities.Core.User> users,
            IEnumerable<Domain.Entities.Products.Product> products)
        {
            var user = users.FirstOrDefault(u => u.Id == redemption.UserId);
            var product = products.FirstOrDefault(p => p.Id == redemption.ProductId);

            return new RedemptionResponseDto
            {
                Id = redemption.Id,
                UserId = redemption.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                UserEmail = user?.Email ?? "",
                ProductId = redemption.ProductId,
                ProductName = product?.Name ?? "Unknown Product",
                PointsSpent = redemption.PointsSpent,
                Quantity = redemption.Quantity,
                Status = redemption.Status.ToString(),
                RequestedAt = redemption.RequestedAt,
                ApprovedAt = redemption.ApprovedAt,
                RejectionReason = redemption.RejectionReason
            };
        }

        public async Task<(IEnumerable<RedemptionResponseDto> Redemptions, int TotalCount)> GetRedemptionsPagedAsync(
            Guid? userId, bool isAdmin, string? statusFilter, int page, int pageSize)
        {
            var allRedemptions = await _unitOfWork.Redemptions.GetAllAsync();
            var redemptions = allRedemptions.AsEnumerable();

            // Filter by user if not admin
            if (!isAdmin && userId.HasValue)
            {
                redemptions = redemptions.Where(r => r.UserId == userId.Value);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<RedemptionStatus>(statusFilter, true, out var statusEnum))
            {
                redemptions = redemptions.Where(r => r.Status == statusEnum);
            }

            var users = await _unitOfWork.Users.GetAllAsync();
            var products = await _unitOfWork.Products.GetAllAsync();

            var redemptionList = redemptions.ToList();
            var totalCount = redemptionList.Count;

            var pagedRedemptions = redemptionList
                .OrderByDescending(r => r.RequestedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => MapToResponseDto(r, users, products))
                .ToList();

            return (pagedRedemptions, totalCount);
        }

        public async Task<(IEnumerable<RedemptionResponseDto> Redemptions, int TotalCount)> GetUserRedemptionsPagedAsync(
            Guid userId, int page, int pageSize)
        {
            var redemptions = await _unitOfWork.Redemptions.FindAsync(r => r.UserId == userId);
            var products = await _unitOfWork.Products.GetAllAsync();

            var redemptionList = redemptions.ToList();
            var totalCount = redemptionList.Count;

            var pagedRedemptions = redemptionList
                .OrderByDescending(r => r.RequestedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RedemptionResponseDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    ProductId = r.ProductId,
                    ProductName = products.FirstOrDefault(p => p.Id == r.ProductId)?.Name ?? "Unknown Product",
                    PointsSpent = r.PointsSpent,
                    Quantity = r.Quantity,
                    Status = r.Status.ToString(),
                    RequestedAt = r.RequestedAt,
                    ApprovedAt = r.ApprovedAt,
                    RejectionReason = r.RejectionReason
                })
                .ToList();

            return (pagedRedemptions, totalCount);
        }

        public async Task<IEnumerable<RedemptionResponseDto>> GetPendingRedemptionsAsync()
        {
            var redemptions = await _unitOfWork.Redemptions.FindAsync(r => r.Status == RedemptionStatus.Pending);
            var users = await _unitOfWork.Users.GetAllAsync();
            var products = await _unitOfWork.Products.GetAllAsync();

            return redemptions
                .Select(r => MapToResponseDto(r, users, products))
                .OrderByDescending(r => r.RequestedAt);
        }

        public async Task<(IEnumerable<RedemptionResponseDto> Redemptions, int TotalCount)> GetRedemptionHistoryPagedAsync(
            int page, int pageSize)
        {
            var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
            var users = await _unitOfWork.Users.GetAllAsync();
            var products = await _unitOfWork.Products.GetAllAsync();

            var redemptionList = redemptions.ToList();
            var totalCount = redemptionList.Count;

            var pagedRedemptions = redemptionList
                .OrderByDescending(r => r.RequestedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => MapToResponseDto(r, users, products))
                .ToList();

            return (pagedRedemptions, totalCount);
        }

        public async Task<int> GetUserPendingRedemptionsCountAsync(Guid userId)
        {
            var redemptions = await _unitOfWork.Redemptions.FindAsync(
                r => r.UserId == userId &&
                     (r.Status == RedemptionStatus.Pending || r.Status == RedemptionStatus.Approved));

            return redemptions.Count();
        }

        public async Task<int> GetProductPendingRedemptionsCountAsync(Guid productId)
        {
            var redemptions = await _unitOfWork.Redemptions.FindAsync(
                r => r.ProductId == productId && r.Status == RedemptionStatus.Pending);

            return redemptions.Count();
        }
    }
}

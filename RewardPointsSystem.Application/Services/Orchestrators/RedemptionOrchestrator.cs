using System;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Operations;

namespace RewardPointsSystem.Application.Services.Orchestrators
{
    /// <summary>
    /// Service: RedemptionOrchestrator
    /// Responsibility: Coordinate redemption flow only
    /// </summary>
    public class RedemptionOrchestrator : IRedemptionOrchestrator
    {
        private readonly IUserPointsAccountService _accountService;
        private readonly IPricingService _pricingService;
        private readonly IInventoryService _inventoryService;
        private readonly IUserPointsTransactionService _transactionService;
        private readonly IUnitOfWork _unitOfWork;

        public RedemptionOrchestrator(
            IUserPointsAccountService accountService,
            IPricingService pricingService,
            IInventoryService inventoryService,
            IUserPointsTransactionService transactionService,
            IUnitOfWork unitOfWork)
        {
            _accountService = accountService;
            _pricingService = pricingService;
            _inventoryService = inventoryService;
            _transactionService = transactionService;
            _unitOfWork = unitOfWork;
        }

        public async Task<RedemptionResult> ProcessRedemptionAsync(Guid userId, Guid productId)
        {
            try
            {
                // 1. Check balance (PointsAccountService)
                var hasAccount = await _accountService.GetAccountAsync(userId) != null;
                if (!hasAccount)
                    throw new InvalidOperationException($"User {userId} does not have a reward account");

                // 2. Get points cost (PricingService)
                var pointsCost = await _pricingService.GetCurrentPointsCostAsync(productId);

                // 3. Check sufficient balance
                var hasSufficientBalance = await _accountService.HasSufficientBalanceAsync(userId, pointsCost);
                if (!hasSufficientBalance)
                {
                    var currentBalance = await _accountService.GetBalanceAsync(userId);
                    throw new InvalidOperationException($"Insufficient balance. Required: {pointsCost}, Available: {currentBalance}");
                }

                // 4. Check stock (InventoryService)
                var isInStock = await _inventoryService.IsInStockAsync(productId);
                if (!isInStock)
                    throw new InvalidOperationException($"Product {productId} is out of stock");

                // 4.1 Validate requested quantity against available inventory
                int quantity = 1; // Default quantity, could be parameterized
                var inventory = await _unitOfWork.Inventory.SingleOrDefaultAsync(i => i.ProductId == productId);
                if (inventory == null)
                    throw new InvalidOperationException($"Inventory not found for product {productId}");
                
                int availableQuantity = inventory.QuantityAvailable - inventory.QuantityReserved;
                if (quantity > availableQuantity)
                    throw new InvalidOperationException($"Insufficient quantity available. Requested: {quantity}, Available: {availableQuantity}");

                // 5. Reserve stock (InventoryService) - using quantity from redemption
                await _inventoryService.ReserveStockAsync(productId, quantity);

                // 6. Deduct user points (UserPointsAccountService)
                await _accountService.DeductUserPointsAsync(userId, pointsCost);

                // 7. Record transaction (UserPointsTransactionService)
                var redemption = new Redemption
                {
                    UserId = userId,
                    ProductId = productId,
                    PointsSpent = pointsCost,
                    Quantity = quantity,
                    Status = RedemptionStatus.Pending,
                    RequestedAt = DateTime.UtcNow
                };

                await _unitOfWork.Redemptions.AddAsync(redemption);
                await _unitOfWork.SaveChangesAsync();

                await _transactionService.RecordRedeemedUserPointsAsync(userId, pointsCost, redemption.Id,
                    $"Product redemption - Redemption ID: {redemption.Id}");

                return new RedemptionResult
                {
                    Success = true,
                    Message = $"Successfully processed redemption request for {pointsCost} points",
                    Redemption = redemption,
                    Transaction = (await _transactionService.GetUserTransactionsAsync(userId))
                        .OrderByDescending(t => t.Timestamp).FirstOrDefault()
                };
            }
            catch (Exception ex)
            {
                return new RedemptionResult
                {
                    Success = false,
                    Message = $"Failed to process redemption: {ex.Message}",
                    Redemption = null,
                    Transaction = null
                };
            }
        }

        public async Task ApproveRedemptionAsync(Guid redemptionId)
        {
            var redemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionId);
            if (redemption == null)
                throw new ArgumentException($"Redemption with ID {redemptionId} not found");

            if (redemption.Status != RedemptionStatus.Pending)
                throw new InvalidOperationException($"Only pending redemptions can be approved. Current status: {redemption.Status}");

            redemption.Status = RedemptionStatus.Approved;
            redemption.ApprovedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeliverRedemptionAsync(Guid redemptionId, string notes)
        {
            var redemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionId);
            if (redemption == null)
                throw new ArgumentException($"Redemption with ID {redemptionId} not found");

            if (redemption.Status != RedemptionStatus.Approved)
                throw new InvalidOperationException($"Only approved redemptions can be delivered. Current status: {redemption.Status}");

            redemption.Status = RedemptionStatus.Delivered;
            redemption.DeliveredAt = DateTime.UtcNow;
            redemption.DeliveryNotes = notes ?? "";

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelRedemptionAsync(Guid redemptionId)
        {
            var redemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionId);
            if (redemption == null)
                throw new ArgumentException($"Redemption with ID {redemptionId} not found");

            if (redemption.Status == RedemptionStatus.Delivered)
                throw new InvalidOperationException("Cannot cancel delivered redemptions");

            if (redemption.Status == RedemptionStatus.Cancelled)
                throw new InvalidOperationException("Redemption is already cancelled");

            // Release reserved stock using the actual quantity from redemption
            await _inventoryService.ReleaseReservationAsync(redemption.ProductId, redemption.Quantity);

            // Refund user points
            await _accountService.AddUserPointsAsync(redemption.UserId, redemption.PointsSpent);

            // Record refund transaction
            await _transactionService.RecordEarnedUserPointsAsync(redemption.UserId, redemption.PointsSpent,
                redemption.Id, $"Redemption cancellation refund - Redemption ID: {redemption.Id}");

            redemption.Status = RedemptionStatus.Cancelled;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
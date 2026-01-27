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

        public async Task<RedemptionResult> ProcessRedemptionAsync(Guid userId, Guid productId, int quantity = 1)
        {
            try
            {
                // Validate quantity
                if (quantity < 1)
                    throw new InvalidOperationException("Quantity must be at least 1");
                if (quantity > 10)
                    throw new InvalidOperationException("Quantity cannot exceed 10 items per redemption");

                // Check for existing pending redemption for this user and product
                var existingRedemptions = await _unitOfWork.Redemptions.GetAllAsync();
                var hasPendingRedemption = existingRedemptions.Any(r => 
                    r.UserId == userId && 
                    r.ProductId == productId && 
                    r.Status == RedemptionStatus.Pending);
                
                if (hasPendingRedemption)
                    throw new InvalidOperationException("You already have a pending redemption request for this product. Please wait for it to be processed before requesting again.");

                // 1. Check balance (PointsAccountService)
                var hasAccount = await _accountService.GetAccountAsync(userId) != null;
                if (!hasAccount)
                    throw new InvalidOperationException($"User {userId} does not have a reward account");

                // 2. Get points cost per unit (PricingService)
                var pointsCostPerUnit = await _pricingService.GetCurrentPointsCostAsync(productId);
                var totalPointsCost = pointsCostPerUnit * quantity;

                // 3. Check sufficient balance for total cost
                var hasSufficientBalance = await _accountService.HasSufficientBalanceAsync(userId, totalPointsCost);
                if (!hasSufficientBalance)
                {
                    var currentBalance = await _accountService.GetBalanceAsync(userId);
                    throw new InvalidOperationException($"Insufficient balance. Required: {totalPointsCost}, Available: {currentBalance}");
                }

                // 4. Check stock (InventoryService)
                var isInStock = await _inventoryService.IsInStockAsync(productId);
                if (!isInStock)
                    throw new InvalidOperationException($"Product {productId} is out of stock");

                // 4.1 Validate requested quantity against available inventory
                var inventory = await _unitOfWork.Inventory.SingleOrDefaultAsync(i => i.ProductId == productId);
                if (inventory == null)
                    throw new InvalidOperationException($"Inventory not found for product {productId}");
                
                int availableQuantity = inventory.QuantityAvailable - inventory.QuantityReserved;
                if (quantity > availableQuantity)
                    throw new InvalidOperationException($"Insufficient quantity available. Requested: {quantity}, Available: {availableQuantity}");

                // 5. Reserve stock (InventoryService) - using requested quantity
                await _inventoryService.ReserveStockAsync(productId, quantity);

                // 6. Deduct user points (UserPointsAccountService) - total cost
                await _accountService.DeductUserPointsAsync(userId, totalPointsCost);

                // 6.1 Add to pending points (tracking for pending redemptions)
                await _accountService.AddPendingPointsAsync(userId, totalPointsCost);

                // 7. Record transaction (UserPointsTransactionService)
                var redemption = Redemption.Create(userId, productId, totalPointsCost, quantity);

                await _unitOfWork.Redemptions.AddAsync(redemption);
                await _unitOfWork.SaveChangesAsync();

                await _transactionService.RecordRedeemedUserPointsAsync(userId, totalPointsCost, redemption.Id,
                    $"Product redemption (Qty: {quantity}) - Redemption ID: {redemption.Id}");

                return new RedemptionResult
                {
                    Success = true,
                    Message = $"Successfully processed redemption request for {totalPointsCost} points (Quantity: {quantity})",
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

        public async Task<Redemption> CreateRedemptionAsync(Guid userId, Guid productId, int quantity = 1)
        {
            var result = await ProcessRedemptionAsync(userId, productId, quantity);
            if (!result.Success)
                throw new InvalidOperationException(result.Message);
            return result.Redemption;
        }

        public async Task ApproveRedemptionAsync(Guid redemptionId, Guid approvedBy)
        {
            var redemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionId);
            if (redemption == null)
                throw new ArgumentException($"Redemption with ID {redemptionId} not found");

            if (redemption.Status != RedemptionStatus.Pending)
                throw new InvalidOperationException($"Only pending redemptions can be approved. Current status: {redemption.Status}");

            redemption.Approve(approvedBy);
            
            // Release pending points when approved (redemption is confirmed, no longer pending)
            await _accountService.ReleasePendingPointsAsync(redemption.UserId, redemption.PointsSpent);
            
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeliverRedemptionAsync(Guid redemptionId, Guid deliveredBy)
        {
            var redemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionId);
            if (redemption == null)
                throw new ArgumentException($"Redemption with ID {redemptionId} not found");

            if (redemption.Status != RedemptionStatus.Approved)
                throw new InvalidOperationException($"Only approved redemptions can be delivered. Current status: {redemption.Status}");

            // Confirm fulfillment - this permanently reduces the reserved stock
            await _inventoryService.ConfirmFulfillmentAsync(redemption.ProductId, redemption.Quantity);

            redemption.Deliver(deliveredBy);
            
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelRedemptionAsync(Guid redemptionId, string reason)
        {
            var redemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionId);
            if (redemption == null)
                throw new ArgumentException($"Redemption with ID {redemptionId} not found");

            if (redemption.Status == RedemptionStatus.Cancelled)
                throw new InvalidOperationException("Redemption is already cancelled");

            // Release reserved stock using the actual quantity from redemption
            await _inventoryService.ReleaseReservationAsync(redemption.ProductId, redemption.Quantity);

            // Cancel pending points and refund to balance (this handles both operations)
            await _accountService.CancelPendingPointsAsync(redemption.UserId, redemption.PointsSpent);

            // Record refund transaction with Redemption source (not Event)
            await _transactionService.RecordRedemptionRefundAsync(redemption.UserId, redemption.PointsSpent,
                redemption.Id, "Points refunded - Redemption cancelled");

            redemption.Cancel(reason ?? "User cancelled redemption");
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RejectRedemptionAsync(Guid redemptionId, string reason)
        {
            var redemption = await _unitOfWork.Redemptions.GetByIdAsync(redemptionId);
            if (redemption == null)
                throw new ArgumentException($"Redemption with ID {redemptionId} not found");

            if (redemption.Status != RedemptionStatus.Pending)
                throw new InvalidOperationException($"Only pending redemptions can be rejected. Current status: {redemption.Status}");

            // Release reserved stock using the actual quantity from redemption
            await _inventoryService.ReleaseReservationAsync(redemption.ProductId, redemption.Quantity);

            // Cancel pending points and refund to balance (this handles both operations)
            await _accountService.CancelPendingPointsAsync(redemption.UserId, redemption.PointsSpent);

            // Record refund transaction with Redemption source (not Event)
            await _transactionService.RecordRedemptionRefundAsync(redemption.UserId, redemption.PointsSpent,
                redemption.Id, "Points refunded - Redemption rejected");

            redemption.Cancel(reason ?? "Rejected by administrator");
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using RewardPointsSystem.Models;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem.Services
{
    public class RedemptionService : IRedemptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryService _inventoryService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public RedemptionService(
            IUnitOfWork unitOfWork, 
            IInventoryService inventoryService,
            IUserService userService,
            IRoleService roleService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        public Redemption RedeemProduct(User user, Product product)
        {
            if (user == null) 
                throw new ArgumentNullException(nameof(user));
            if (product == null) 
                throw new ArgumentNullException(nameof(product));

            // Check if user has permission to redeem
            if (!user.IsActive)
                throw new InvalidOperationException("User account is inactive");

            // Validate product is active
            if (!product.IsActive)
                throw new InvalidOperationException("Product is no longer available");

            // Check user has sufficient balance
            if (product.RequiredPoints > user.PointsBalance)
                throw new InvalidOperationException($"Insufficient balance. Required: {product.RequiredPoints}, Available: {user.PointsBalance}");

            // Check inventory availability
            if (!_inventoryService.CheckAvailability(product.Id, 1))
                throw new InvalidOperationException("Product is out of stock");

            try
            {
                // Reserve the stock
                _inventoryService.ReserveStock(product.Id, 1);

                // Deduct points from user
                user.DeductPoints(product.RequiredPoints);
                _unitOfWork.Users.Update(user);

                // Create redemption record
                var redemption = new Redemption
                {
                    User = user,
                    Product = product
                };
                _unitOfWork.Redemptions.Add(redemption);

                // Create points transaction
                var transaction = new PointsTransaction(user, -product.RequiredPoints, "Redeem", $"Redeemed: {product.Name}");
                _unitOfWork.PointsTransactions.Add(transaction);

                // Confirm the stock reservation
                _inventoryService.ConfirmReservation(product.Id, 1);

                // Commit all changes
                _unitOfWork.Complete();

                return redemption;
            }
            catch (Exception)
            {
                // If anything fails, cancel the reservation
                try
                {
                    _inventoryService.CancelReservation(product.Id, 1);
                }
                catch
                {
                    // Log error but don't throw, as the main operation already failed
                }
                throw;
            }
        }

        public IEnumerable<Redemption> GetUserRedemptions(Guid userId)
        {
            return _unitOfWork.Redemptions.Find(r => r.User.Id == userId);
        }

        public IEnumerable<Redemption> GetAllRedemptions()
        {
            return _unitOfWork.Redemptions.GetAll();
        }

        public Redemption GetRedemptionById(Guid id)
        {
            return _unitOfWork.Redemptions.GetById(id);
        }
    }
}


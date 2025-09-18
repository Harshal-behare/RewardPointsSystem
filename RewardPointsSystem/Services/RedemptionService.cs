using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RewardPointsSystem.Models;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem.Services
{
    public class RedemptionService : IRedemptionService
    {
        private readonly List<Redemption> _redemptions = new();
        private readonly PointsTransactionService _transactionService;

        public RedemptionService(PointsTransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public Redemption RedeemProduct(User user, Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (product.RequiredPoints > user.PointsBalance)
                throw new InvalidOperationException("Insufficient balance");
            if (product.Stock <= 0)
                throw new InvalidOperationException("Product out of stock");

            user.DeductPoints(product.RequiredPoints);
            product.Stock--;

            var redemption = new Redemption { User = user, Product = product };
            _redemptions.Add(redemption);

            // Log transaction
            _transactionService.AddTransaction(user, product.RequiredPoints, "Redeem");

            return redemption;
        }
    }

}


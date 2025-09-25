using System;
using System.Collections.Generic;
using RewardPointsSystem.Models;

namespace RewardPointsSystem.Interfaces
{
    public interface IRedemptionService
    {
        Redemption RedeemProduct(User user, Product product);
        IEnumerable<Redemption> GetUserRedemptions(Guid userId);
        IEnumerable<Redemption> GetAllRedemptions();
        Redemption GetRedemptionById(Guid id);
    }
}


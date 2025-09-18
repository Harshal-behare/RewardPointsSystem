using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RewardPointsSystem.Models;

namespace RewardPointsSystem.Interfaces
{
    public interface IRedemptionService
    {
        Redemption RedeemProduct(User user, Product product);
    }
}


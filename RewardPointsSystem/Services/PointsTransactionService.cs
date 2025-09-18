using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RewardPointsSystem.Models;

namespace RewardPointsSystem.Services
{
    public class PointsTransactionService
    {
        private readonly List<PointsTransaction> _transactions = new();

        public void AddTransaction(User user, int points, string type)
        {
            var transaction = new PointsTransaction
            {
                User = user,
                Points = points,
                Type = type
            };
            _transactions.Add(transaction);
        }

        public IEnumerable<PointsTransaction> GetAllTransactions() => _transactions;
    }
}


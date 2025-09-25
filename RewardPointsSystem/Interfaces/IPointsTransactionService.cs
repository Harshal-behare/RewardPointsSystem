using System;
using System.Collections.Generic;
using RewardPointsSystem.Models;

namespace RewardPointsSystem.Interfaces
{
    public interface IPointsTransactionService
    {
        PointsTransaction AddTransaction(User user, int points, string type, string description = "");
        IEnumerable<PointsTransaction> GetUserTransactions(Guid userId);
        IEnumerable<PointsTransaction> GetAllTransactions();
        IEnumerable<PointsTransaction> GetTransactionsByType(string type);
        int GetUserBalance(Guid userId);
    }
}
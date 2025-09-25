using System;
using System.Collections.Generic;
using System.Linq;
using RewardPointsSystem.Models;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem.Services
{
    public class PointsTransactionService : IPointsTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PointsTransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public PointsTransaction AddTransaction(User user, int points, string type, string description = "")
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var transaction = new PointsTransaction(user, points, type, description);
            _unitOfWork.PointsTransactions.Add(transaction);
            _unitOfWork.Complete();
            
            return transaction;
        }

        public IEnumerable<PointsTransaction> GetUserTransactions(Guid userId)
        {
            return _unitOfWork.PointsTransactions.Find(t => t.User.Id == userId)
                .OrderByDescending(t => t.Timestamp);
        }

        public IEnumerable<PointsTransaction> GetAllTransactions()
        {
            return _unitOfWork.PointsTransactions.GetAll()
                .OrderByDescending(t => t.Timestamp);
        }

        public IEnumerable<PointsTransaction> GetTransactionsByType(string type)
        {
            return _unitOfWork.PointsTransactions.Find(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.Timestamp);
        }

        public int GetUserBalance(Guid userId)
        {
            var user = _unitOfWork.Users.GetById(userId);
            return user?.PointsBalance ?? 0;
        }
    }
}

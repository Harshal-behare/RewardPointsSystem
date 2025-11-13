using System;

namespace RewardPointsSystem.Application.DTOs.Points
{
    /// <summary>
    /// DTO for points account response
    /// </summary>
    public class PointsAccountResponseDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int CurrentBalance { get; set; }
        public int TotalEarned { get; set; }
        public int TotalRedeemed { get; set; }
        public DateTime LastTransaction { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for transaction response
    /// </summary>
    public class TransactionResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TransactionType { get; set; }
        public int UserPoints { get; set; }
        public string Description { get; set; }
        public Guid? EventId { get; set; }
        public string EventName { get; set; }
        public Guid? RedemptionId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// DTO for adding points
    /// </summary>
    public class AddPointsDto
    {
        public Guid UserId { get; set; }
        public int Points { get; set; }
        public string Description { get; set; }
        public Guid? EventId { get; set; }
    }

    /// <summary>
    /// DTO for deducting points
    /// </summary>
    public class DeductPointsDto
    {
        public Guid UserId { get; set; }
        public int Points { get; set; }
        public string Reason { get; set; }
    }
}

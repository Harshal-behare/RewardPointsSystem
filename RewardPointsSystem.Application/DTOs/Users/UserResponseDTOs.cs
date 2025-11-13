using System;
using System.Collections.Generic;

namespace RewardPointsSystem.Application.DTOs.Users
{
    /// <summary>
    /// Basic user response DTO
    /// </summary>
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Detailed user response with relationships
    /// </summary>
    public class UserDetailsDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; }
        public int PointsBalance { get; set; }
        public int TotalPointsEarned { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public int EventsParticipated { get; set; }
        public int RedemptionsCount { get; set; }
    }

    /// <summary>
    /// User with points balance
    /// </summary>
    public class UserBalanceDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int CurrentBalance { get; set; }
        public int TotalEarned { get; set; }
        public int TotalRedeemed { get; set; }
        public DateTime LastTransaction { get; set; }
    }
}

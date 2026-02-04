using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Users;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IUserQueryService
    /// Responsibility: Query user data with related entities (read operations)
    /// Clean Architecture - Separates read concerns from IUserService
    /// </summary>
    public interface IUserQueryService
    {
        /// <summary>
        /// Get all users with their roles and points balances (for admin view)
        /// </summary>
        Task<IEnumerable<UserResponseDto>> GetAllUsersWithDetailsAsync();
        
        /// <summary>
        /// Get user details by ID including roles and points
        /// </summary>
        Task<UserResponseDto?> GetUserWithDetailsAsync(Guid userId);
        
        /// <summary>
        /// Get active users with their roles and points balances
        /// </summary>
        Task<IEnumerable<UserResponseDto>> GetActiveUsersWithDetailsAsync();
    }
}

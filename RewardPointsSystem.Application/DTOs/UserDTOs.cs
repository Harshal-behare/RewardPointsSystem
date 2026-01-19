using System;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Application.DTOs
{
    /// <summary>
    /// DTO for creating a new user
    /// </summary>
    public class CreateUserDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; }

        /// <summary>
        /// Initial password for the user (optional - if not provided, a random password will be generated)
        /// </summary>
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        /// <summary>
        /// Role to assign to the user (default: Employee)
        /// </summary>
        public string Role { get; set; } = "Employee";
    }

    /// <summary>
    /// DTO for updating an existing user
    /// </summary>
    public class UpdateUserDto
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
        public string FirstName { get; set; }

        [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; }

        /// <summary>
        /// Active/Inactive status
        /// </summary>
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for user update operations (duplicate of UpdateUserDto - consider consolidating)
    /// </summary>
    public class UserUpdateDto
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
        public string FirstName { get; set; }

        [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; }
    }
}

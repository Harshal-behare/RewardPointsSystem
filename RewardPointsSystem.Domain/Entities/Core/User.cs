using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Domain.Entities.Core
{
    /// <summary>
    /// Represents a user in the reward points system
    /// </summary>
    public class User
    {
        private readonly List<UserRole> _userRoles;
        private readonly List<EventParticipant> _eventParticipations;
        private readonly List<Redemption> _redemptions;

        public Guid Id { get; private set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; private set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
        public string FirstName { get; private set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
        public string LastName { get; private set; }

        [StringLength(500, ErrorMessage = "Password hash cannot exceed 500 characters")]
        public string? PasswordHash { get; private set; }

        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        // Navigation Properties - Encapsulated collections
        public virtual IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
        public virtual UserPointsAccount? UserPointsAccount { get; private set; }
        public virtual IReadOnlyCollection<EventParticipant> EventParticipations => _eventParticipations.AsReadOnly();
        public virtual IReadOnlyCollection<Redemption> Redemptions => _redemptions.AsReadOnly();

        // EF Core requires a parameterless constructor
        private User()
        {
            _userRoles = new List<UserRole>();
            _eventParticipations = new List<EventParticipant>();
            _redemptions = new List<Redemption>();
            Email = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
        }

        private User(string email, string firstName, string lastName) : this()
        {
            Id = Guid.NewGuid();
            Email = ValidateEmail(email);
            FirstName = ValidateName(firstName, nameof(firstName));
            LastName = ValidateName(lastName, nameof(lastName));
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method to create a new user
        /// </summary>
        public static User Create(string email, string firstName, string lastName)
        {
            return new User(email, firstName, lastName);
        }

        /// <summary>
        /// Updates user information
        /// </summary>
        public void UpdateInfo(string email, string firstName, string lastName, Guid updatedBy)
        {
            Email = ValidateEmail(email);
            FirstName = ValidateName(firstName, nameof(firstName));
            LastName = ValidateName(lastName, nameof(lastName));
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Activates the user account
        /// </summary>
        public void Activate(Guid activatedBy)
        {
            if (IsActive)
                throw new InvalidUserOperationException($"User '{Id}' is already active.");

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = activatedBy;
        }

        /// <summary>
        /// Deactivates the user account
        /// </summary>
        public void Deactivate(Guid deactivatedBy)
        {
            if (!IsActive)
                throw new InvalidUserOperationException($"User '{Id}' is already inactive.");

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deactivatedBy;
        }

        /// <summary>
        /// Checks if user has a specific role
        /// </summary>
        public bool HasRole(Guid roleId)
        {
            return _userRoles.Any(ur => ur.RoleId == roleId && ur.IsActive);
        }

        /// <summary>
        /// Adds a role to the user (internal use for EF navigation)
        /// </summary>
        internal void AddRole(UserRole userRole)
        {
            if (userRole == null)
                throw new ArgumentNullException(nameof(userRole));

            if (_userRoles.Any(ur => ur.RoleId == userRole.RoleId && ur.IsActive))
                throw new InvalidUserOperationException($"User '{Id}' already has role '{userRole.RoleId}'.");

            _userRoles.Add(userRole);
        }

        /// <summary>
        /// Sets the points account (internal use for EF navigation)
        /// </summary>
        internal void SetPointsAccount(UserPointsAccount account)
        {
            UserPointsAccount = account ?? throw new ArgumentNullException(nameof(account));
        }

        /// <summary>
        /// Sets the password hash for the user
        /// </summary>
        public void SetPasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

            if (passwordHash.Length > 500)
                throw new ArgumentException("Password hash cannot exceed 500 characters.", nameof(passwordHash));

            PasswordHash = passwordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the user has a password set
        /// </summary>
        public bool HasPassword()
        {
            return !string.IsNullOrWhiteSpace(PasswordHash);
        }

        private static string ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            if (email.Length > 255)
                throw new ArgumentException("Email cannot exceed 255 characters.", nameof(email));

            return email.Trim().ToLowerInvariant();
        }

        private static string ValidateName(string name, string paramName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"{paramName} is required.", paramName);

            if (name.Length > 100)
                throw new ArgumentException($"{paramName} cannot exceed 100 characters.", paramName);

            return name.Trim();
        }
    }
}

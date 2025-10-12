using System;

namespace RewardPointsSystem.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a user is not found.
    /// </summary>
    public class UserNotFoundException : DomainException
    {
        public Guid UserId { get; }

        public UserNotFoundException(Guid userId) 
            : base($"User with ID '{userId}' was not found.")
        {
            UserId = userId;
        }

        public UserNotFoundException(string email) 
            : base($"User with email '{email}' was not found.")
        {
        }
    }

    /// <summary>
    /// Exception thrown when attempting to create a user with a duplicate email.
    /// </summary>
    public class DuplicateUserEmailException : DomainException
    {
        public string Email { get; }

        public DuplicateUserEmailException(string email) 
            : base($"User with email '{email}' already exists.")
        {
            Email = email;
        }
    }

    /// <summary>
    /// Exception thrown when user data is invalid.
    /// </summary>
    public class InvalidUserDataException : DomainException
    {
        public InvalidUserDataException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when attempting to operate on an inactive user.
    /// </summary>
    public class InactiveUserException : DomainException
    {
        public Guid UserId { get; }

        public InactiveUserException(Guid userId) 
            : base($"User with ID '{userId}' is inactive and cannot perform this operation.")
        {
            UserId = userId;
        }
    }
}

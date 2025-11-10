using System;
using System.Text.RegularExpressions;

namespace RewardPointsSystem.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing an email address
    /// </summary>
    public sealed class Email : IEquatable<Email>
    {
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));

            if (email.Length > 255)
                throw new ArgumentException("Email cannot exceed 255 characters.", nameof(email));

            var normalizedEmail = email.Trim().ToLowerInvariant();

            if (!EmailRegex.IsMatch(normalizedEmail))
                throw new ArgumentException("Invalid email format.", nameof(email));

            return new Email(normalizedEmail);
        }

        public bool Equals(Email? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object? obj) => obj is Email other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;

        public static bool operator ==(Email? left, Email? right) => Equals(left, right);
        public static bool operator !=(Email? left, Email? right) => !Equals(left, right);

        public static implicit operator string(Email email) => email.Value;
    }
}

using System;

namespace RewardPointsSystem.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing a person's name
    /// </summary>
    public sealed class PersonName : IEquatable<PersonName>
    {
        public string FirstName { get; }
        public string LastName { get; }

        private PersonName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public static PersonName Create(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty.", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

            if (firstName.Length > 100)
                throw new ArgumentException("First name cannot exceed 100 characters.", nameof(firstName));

            if (lastName.Length > 100)
                throw new ArgumentException("Last name cannot exceed 100 characters.", nameof(lastName));

            return new PersonName(firstName.Trim(), lastName.Trim());
        }

        public string GetFullName() => $"{FirstName} {LastName}";

        public bool Equals(PersonName? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return FirstName == other.FirstName && LastName == other.LastName;
        }

        public override bool Equals(object? obj) => obj is PersonName other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(FirstName, LastName);

        public override string ToString() => GetFullName();

        public static bool operator ==(PersonName? left, PersonName? right) => Equals(left, right);
        public static bool operator !=(PersonName? left, PersonName? right) => !Equals(left, right);
    }
}

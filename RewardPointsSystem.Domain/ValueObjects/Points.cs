using System;

namespace RewardPointsSystem.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing a points amount
    /// </summary>
    public sealed class Points : IEquatable<Points>, IComparable<Points>
    {
        public int Value { get; }

        private Points(int value)
        {
            Value = value;
        }

        public static Points Create(int value)
        {
            if (value < 0)
                throw new ArgumentException("Points cannot be negative.", nameof(value));

            return new Points(value);
        }

        public static Points Zero => new(0);

        public Points Add(Points other) => new(Value + other.Value);

        public Points Subtract(Points other)
        {
            if (Value < other.Value)
                throw new InvalidOperationException("Resulting points cannot be negative.");

            return new(Value - other.Value);
        }

        public bool IsGreaterThan(Points other) => Value > other.Value;
        public bool IsGreaterThanOrEqual(Points other) => Value >= other.Value;
        public bool IsLessThan(Points other) => Value < other.Value;
        public bool IsLessThanOrEqual(Points other) => Value <= other.Value;

        public bool Equals(Points? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object? obj) => obj is Points other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => $"{Value} points";

        public int CompareTo(Points? other)
        {
            if (other is null) return 1;
            return Value.CompareTo(other.Value);
        }

        public static bool operator ==(Points? left, Points? right) => Equals(left, right);
        public static bool operator !=(Points? left, Points? right) => !Equals(left, right);
        public static bool operator >(Points left, Points right) => left.Value > right.Value;
        public static bool operator <(Points left, Points right) => left.Value < right.Value;
        public static bool operator >=(Points left, Points right) => left.Value >= right.Value;
        public static bool operator <=(Points left, Points right) => left.Value <= right.Value;

        public static implicit operator int(Points points) => points.Value;
    }
}

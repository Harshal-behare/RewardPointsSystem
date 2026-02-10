using System;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Domain.Entities.Events
{
    public enum AttendanceStatus
    {
        Registered,
        CheckedIn,
        Attended,
        NoShow,
        Cancelled
    }

    /// <summary>
    /// Represents a user's participation in an event with business logic
    /// </summary>
    public class EventParticipant
    {
        public Guid Id { get; private set; }

        public Guid EventId { get; private set; }

        public Guid UserId { get; private set; }

        public int? PointsAwarded { get; private set; }

        public int? EventRank { get; private set; }

        public AttendanceStatus AttendanceStatus { get; private set; }

        public DateTime RegisteredAt { get; private set; }
        public DateTime? CheckedInAt { get; private set; }
        public DateTime? AwardedAt { get; private set; }
        public Guid? AwardedBy { get; private set; }

        // Navigation Properties
        public virtual Event? Event { get; private set; }
        public virtual User? User { get; private set; }

        // EF Core requires a parameterless constructor
        private EventParticipant()
        {
        }

        private EventParticipant(Guid eventId, Guid userId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            UserId = userId;
            RegisteredAt = DateTime.UtcNow;
            AttendanceStatus = AttendanceStatus.Registered;
        }

        /// <summary>
        /// Factory method to register a user for an event
        /// </summary>
        public static EventParticipant Register(Guid eventId, Guid userId)
        {
            if (eventId == Guid.Empty)
                throw new ArgumentException("Event ID cannot be empty.", nameof(eventId));

            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            return new EventParticipant(eventId, userId);
        }

        /// <summary>
        /// Checks in the participant (Registered → CheckedIn)
        /// </summary>
        public void CheckIn()
        {
            if (AttendanceStatus != AttendanceStatus.Registered)
                throw new InvalidEventStateException(EventId, 
                    $"Cannot check in participant with status {AttendanceStatus}.");

            AttendanceStatus = AttendanceStatus.CheckedIn;
            CheckedInAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks participant as attended (CheckedIn → Attended)
        /// </summary>
        public void MarkAsAttended()
        {
            if (AttendanceStatus != AttendanceStatus.CheckedIn)
                throw new InvalidEventStateException(EventId,
                    $"Cannot mark as attended. Current status: {AttendanceStatus}. Must be CheckedIn first.");

            AttendanceStatus = AttendanceStatus.Attended;
        }

        /// <summary>
        /// Marks participant as no-show
        /// </summary>
        public void MarkAsNoShow()
        {
            if (AttendanceStatus == AttendanceStatus.Cancelled)
                throw new InvalidEventStateException(EventId, "Cannot mark cancelled participant as no-show.");

            if (AttendanceStatus == AttendanceStatus.Attended)
                throw new InvalidEventStateException(EventId, "Cannot mark attended participant as no-show.");

            if (PointsAwarded.HasValue)
                throw new InvalidEventStateException(EventId, "Cannot mark as no-show after points have been awarded.");

            AttendanceStatus = AttendanceStatus.NoShow;
        }

        /// <summary>
        /// Cancels the participation
        /// </summary>
        public void Cancel()
        {
            if (AttendanceStatus == AttendanceStatus.Attended)
                throw new InvalidEventStateException(EventId, "Cannot cancel after attending the event.");

            if (PointsAwarded.HasValue)
                throw new InvalidEventStateException(EventId, "Cannot cancel after points have been awarded.");

            AttendanceStatus = AttendanceStatus.Cancelled;
        }

        /// <summary>
        /// Awards points to the participant
        /// </summary>
        public void AwardPoints(int points, int? rank, Guid awardedBy)
        {
            if (AttendanceStatus != AttendanceStatus.Attended && AttendanceStatus != AttendanceStatus.CheckedIn)
                throw new InvalidEventStateException(EventId,
                    $"Cannot award points to participant with status {AttendanceStatus}. Must be CheckedIn or Attended.");

            if (PointsAwarded.HasValue)
                throw new DuplicatePointsAwardException(UserId, EventId);

            if (points < 0)
                throw new ArgumentException("Points awarded cannot be negative.", nameof(points));

            if (rank.HasValue && rank.Value < 1)
                throw new ArgumentException("Event rank must be a positive number.", nameof(rank));

            PointsAwarded = points;
            EventRank = rank;
            AwardedAt = DateTime.UtcNow;
            AwardedBy = awardedBy;

            // Automatically mark as attended if they were only checked in
            if (AttendanceStatus == AttendanceStatus.CheckedIn)
            {
                AttendanceStatus = AttendanceStatus.Attended;
            }
        }

        /// <summary>
        /// Revokes previously awarded points (for corrections)
        /// </summary>
        public void RevokePoints()
        {
            if (!PointsAwarded.HasValue)
                throw new InvalidEventStateException(EventId, "No points have been awarded to revoke.");

            PointsAwarded = null;
            EventRank = null;
            AwardedAt = null;
            AwardedBy = null;
        }

        /// <summary>
        /// Checks if points have been awarded
        /// </summary>
        public bool HasPointsAwarded() => PointsAwarded.HasValue;

        /// <summary>
        /// Checks if participant is eligible for points
        /// </summary>
        public bool IsEligibleForPoints() => 
            (AttendanceStatus == AttendanceStatus.Attended || AttendanceStatus == AttendanceStatus.CheckedIn) &&
            !PointsAwarded.HasValue;
    }
}

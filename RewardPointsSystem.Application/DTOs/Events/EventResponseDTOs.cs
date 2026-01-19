using System;
using System.Collections.Generic;

namespace RewardPointsSystem.Application.DTOs.Events
{
    /// <summary>
    /// Basic event response DTO
    /// </summary>
    public class EventResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Status { get; set; }
        public int TotalPointsPool { get; set; }
        public int RemainingPoints { get; set; }
        public int ParticipantsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for changing event status
    /// </summary>
    public class ChangeEventStatusDto
    {
        /// <summary>
        /// Target status: Published, Active, Completed, or Cancelled
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// Detailed event response with participants
    /// </summary>
    public class EventDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Status { get; set; }
        public int TotalPointsPool { get; set; }
        public int RemainingPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<EventParticipantResponseDto> Participants { get; set; }
        public List<PointsAwardedDto> PointsAwarded { get; set; }
    }

    /// <summary>
    /// DTO for registering a participant
    /// </summary>
    public class RegisterParticipantDto
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
    }

    /// <summary>
    /// DTO for awarding points to winners
    /// </summary>
    public class AwardPointsDto
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public int Points { get; set; }
        public int Position { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// DTO for event participant response
    /// </summary>
    public class EventParticipantResponseDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string Status { get; set; }
        public bool HasAttended { get; set; }
        public int? PointsAwarded { get; set; }
        public int? EventRank { get; set; }
    }

    /// <summary>
    /// DTO for points awarded information
    /// </summary>
    public class PointsAwardedDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int Points { get; set; }
        public int Position { get; set; }
        public DateTime AwardedAt { get; set; }
    }
}

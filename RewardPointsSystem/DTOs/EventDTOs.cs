using System;

namespace RewardPointsSystem.DTOs
{
    public class CreateEventDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PointsReward { get; set; }
        public int? MaxParticipants { get; set; }
    }

    public class UpdateEventDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PointsReward { get; set; }
        public int? MaxParticipants { get; set; }
    }

    public class EventUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PointsReward { get; set; }
        public int? MaxParticipants { get; set; }
    }

    public class WinnerDto
    {
        public Guid UserId { get; set; }
        public int Points { get; set; }
        public int Position { get; set; }
    }
}
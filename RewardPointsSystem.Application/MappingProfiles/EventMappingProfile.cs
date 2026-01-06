using AutoMapper;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Events;
using RewardPointsSystem.Domain.Entities.Events;
using System.Linq;

namespace RewardPointsSystem.Application.MappingProfiles
{
    /// <summary>
    /// AutoMapper profile for Event entity mappings
    /// </summary>
    public class EventMappingProfile : Profile
    {
        public EventMappingProfile()
        {
            // Event → EventResponseDto
            CreateMap<Event, EventResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.RemainingPoints, opt => opt.MapFrom(src => src.GetAvailablePointsPool()))
                .ForMember(dest => dest.ParticipantsCount, opt => opt.MapFrom(src => src.Participants.Count));

            // Event → EventDetailsDto
            CreateMap<Event, EventDetailsDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.RemainingPoints, opt => opt.MapFrom(src => src.GetAvailablePointsPool()))
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.Participants))
                .ForMember(dest => dest.PointsAwarded, opt => opt.MapFrom(src => src.Participants
                    .Where(p => p.PointsAwarded.HasValue)
                    .Select(p => new PointsAwardedDto
                    {
                        UserId = p.UserId,
                        UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}" : string.Empty,
                        Points = p.PointsAwarded.Value,
                        Position = p.EventRank ?? 0,
                        AwardedAt = p.AwardedAt ?? p.RegisteredAt
                    })));

            // EventParticipant → EventParticipantResponseDto
            CreateMap<EventParticipant, EventParticipantResponseDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty));

            // CreateEventDto → Event (for reference - use Event.Create() factory method in services)
            CreateMap<CreateEventDto, Event>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.Participants, opt => opt.Ignore());
        }
    }
}

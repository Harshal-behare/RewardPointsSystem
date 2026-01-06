using AutoMapper;
using RewardPointsSystem.Application.DTOs.Points;
using RewardPointsSystem.Domain.Entities.Accounts;
using System.Linq;

namespace RewardPointsSystem.Application.MappingProfiles
{
    /// <summary>
    /// AutoMapper profile for Points and Account entity mappings
    /// </summary>
    public class PointsMappingProfile : Profile
    {
        public PointsMappingProfile()
        {
            // UserPointsAccount → PointsAccountResponseDto
            CreateMap<UserPointsAccount, PointsAccountResponseDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.LastTransaction, opt => opt.MapFrom(src => src.LastUpdatedAt));

            // UserPointsTransaction → TransactionResponseDto
            CreateMap<UserPointsTransaction, TransactionResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.TransactionType.ToString()))
                .ForMember(dest => dest.EventName, opt => opt.Ignore()) // Event navigation not available
                .ForMember(dest => dest.EventId, opt => opt.MapFrom(src => src.TransactionSource == TransactionOrigin.Event ? (Guid?)src.SourceId : null))
                .ForMember(dest => dest.RedemptionId, opt => opt.MapFrom(src => src.TransactionSource == TransactionOrigin.Redemption ? (Guid?)src.SourceId : null));
        }
    }
}

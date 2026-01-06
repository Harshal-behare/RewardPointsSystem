using AutoMapper;
using RewardPointsSystem.Application.DTOs.Redemptions;
using RewardPointsSystem.Domain.Entities.Operations;
using System.Linq;

namespace RewardPointsSystem.Application.MappingProfiles
{
    /// <summary>
    /// AutoMapper profile for Redemption entity mappings
    /// </summary>
    public class RedemptionMappingProfile : Profile
    {
        public RedemptionMappingProfile()
        {
            // Redemption → RedemptionResponseDto
            CreateMap<Redemption, RedemptionResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Redemption → RedemptionDetailsDto
            CreateMap<Redemption, RedemptionDetailsDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductCategory, opt => opt.MapFrom(src => src.Product != null && src.Product.ProductCategory != null ? src.Product.ProductCategory.Name : string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.Approver != null ? $"{src.Approver.FirstName} {src.Approver.LastName}" : string.Empty))
                .ForMember(dest => dest.CancelledAt, opt => opt.MapFrom(src => src.Status == RedemptionStatus.Cancelled ? src.RequestedAt : (DateTime?)null))
                .ForMember(dest => dest.CancellationReason, opt => opt.MapFrom(src => src.RejectionReason));

            // CreateRedemptionDto → Redemption (for reference - use Redemption.Create() factory method in services)
            CreateMap<CreateRedemptionDto, Redemption>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PointsSpent, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.RequestedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveredAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryNotes, opt => opt.Ignore())
                .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Approver, opt => opt.Ignore())
                .ForMember(dest => dest.Processor, opt => opt.Ignore());
        }
    }
}

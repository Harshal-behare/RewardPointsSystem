using AutoMapper;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Users;
using RewardPointsSystem.Domain.Entities.Core;
using System.Linq;

namespace RewardPointsSystem.Application.MappingProfiles
{
    /// <summary>
    /// AutoMapper profile for User entity mappings
    /// </summary>
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // ===================================================
            // ENTITY TO DTO MAPPINGS (for reading/responses)
            // ===================================================

            // User → UserResponseDto (basic user information)
            CreateMap<User, UserResponseDto>();

            // User → UserDetailsDto (detailed user information with relationships)
            CreateMap<User, UserDetailsDto>()
                .ForMember(dest => dest.Roles, 
                    opt => opt.MapFrom(src => src.UserRoles
                        .Where(ur => ur.IsActive && ur.Role != null)
                        .Select(ur => ur.Role!.Name)))
                .ForMember(dest => dest.PointsBalance, 
                    opt => opt.MapFrom(src => src.UserPointsAccount != null ? src.UserPointsAccount.CurrentBalance : 0))
                .ForMember(dest => dest.TotalPointsEarned, 
                    opt => opt.MapFrom(src => src.UserPointsAccount != null ? src.UserPointsAccount.TotalEarned : 0))
                .ForMember(dest => dest.TotalPointsRedeemed, 
                    opt => opt.MapFrom(src => src.UserPointsAccount != null ? src.UserPointsAccount.TotalRedeemed : 0))
                .ForMember(dest => dest.EventsParticipated, 
                    opt => opt.MapFrom(src => src.EventParticipations.Count))
                .ForMember(dest => dest.RedemptionsCount, 
                    opt => opt.MapFrom(src => src.Redemptions.Count));

            // User → UserBalanceDto (user with points balance)
            CreateMap<User, UserBalanceDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CurrentBalance, 
                    opt => opt.MapFrom(src => src.UserPointsAccount != null ? src.UserPointsAccount.CurrentBalance : 0))
                .ForMember(dest => dest.TotalEarned, 
                    opt => opt.MapFrom(src => src.UserPointsAccount != null ? src.UserPointsAccount.TotalEarned : 0))
                .ForMember(dest => dest.TotalRedeemed, 
                    opt => opt.MapFrom(src => src.UserPointsAccount != null ? src.UserPointsAccount.TotalRedeemed : 0))
                .ForMember(dest => dest.LastTransaction, 
                    opt => opt.MapFrom(src => src.UserPointsAccount != null ? src.UserPointsAccount.LastUpdatedAt : src.CreatedAt));

            // ===================================================
            // DTO TO ENTITY MAPPINGS (for create/update operations)
            // ===================================================

            // CreateUserDto → User (using factory method)
            // Note: This is for reference only. Use User.Create() factory method in services
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.UserPointsAccount, opt => opt.Ignore())
                .ForMember(dest => dest.EventParticipations, opt => opt.Ignore())
                .ForMember(dest => dest.Redemptions, opt => opt.Ignore());

            // UpdateUserDto → User (for partial updates)
            // Note: This maps only non-null properties. Use User.UpdateInfo() in services
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.UserPointsAccount, opt => opt.Ignore())
                .ForMember(dest => dest.EventParticipations, opt => opt.Ignore())
                .ForMember(dest => dest.Redemptions, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

using AutoMapper;
using RewardPointsSystem.Application.DTOs.Roles;
using RewardPointsSystem.Domain.Entities.Core;
using System.Linq;

namespace RewardPointsSystem.Application.MappingProfiles
{
    /// <summary>
    /// AutoMapper profile for Role entity mappings
    /// </summary>
    public class RoleMappingProfile : Profile
    {
        public RoleMappingProfile()
        {
            // Role → RoleResponseDto
            CreateMap<Role, RoleResponseDto>();

            // CreateRoleDto → Role (for reference - use Role.Create() factory method in services)
            CreateMap<CreateRoleDto, Role>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore());

            // UserRole → UserRoleResponseDto
            CreateMap<UserRole, UserRoleResponseDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
                .ForMember(dest => dest.AssignedAt, opt => opt.MapFrom(src => src.AssignedAt));
        }
    }
}

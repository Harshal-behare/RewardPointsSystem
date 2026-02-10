using AutoMapper;
using RewardPointsSystem.Application.DTOs.Common;
using System.Collections.Generic;
using System.Linq;

namespace RewardPointsSystem.Application.MappingProfiles
{
    /// <summary>
    /// AutoMapper profile for common/cross-cutting mappings
    /// </summary>
    public class CommonMappingProfile : Profile
    {
        public CommonMappingProfile()
        {
            // Generic paged response mapping
            // This allows creating a PagedResponse<TDto> from any IEnumerable<TSource>
            // Usage in services: mapper.Map<PagedResponse<UserResponseDto>>(pagedUsers);
            
            // Note: Due to AutoMapper's limitations with generic types, 
            // we typically create PagedResponse manually in services rather than mapping it

            // Example helper method pattern to create in services:
            // var pagedResponse = new PagedResponse<UserResponseDto>
            // {
            //     Data = _mapper.Map<List<UserResponseDto>>(users),
            //     PageNumber = pageNumber,
            //     PageSize = pageSize,
            //     TotalCount = totalCount,
            //     TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            // };
        }
    }
}

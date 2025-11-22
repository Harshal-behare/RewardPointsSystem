using AutoMapper;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Products;
using RewardPointsSystem.Domain.Entities.Products;
using System.Linq;

namespace RewardPointsSystem.Application.MappingProfiles
{
    /// <summary>
    /// AutoMapper profile for Product entity mappings
    /// </summary>
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // Product → ProductResponseDto
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.CurrentPointsCost, opt => opt.MapFrom(src => src.GetCurrentPricing() != null ? src.GetCurrentPricing()!.PointsCost : 0))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.ProductCategory != null ? src.ProductCategory.Name : src.Category))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.QuantityAvailable : 0))
                .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.Inventory != null && src.Inventory.QuantityAvailable > 0));

            // Product → ProductDetailsDto
            CreateMap<Product, ProductDetailsDto>()
                .ForMember(dest => dest.CurrentPointsCost, opt => opt.MapFrom(src => src.GetCurrentPricing() != null ? src.GetCurrentPricing()!.PointsCost : 0))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.ProductCategory != null ? src.ProductCategory.Name : src.Category))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.QuantityAvailable : 0))
                .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.Inventory != null && src.Inventory.QuantityAvailable > 0))
                .ForMember(dest => dest.ReorderLevel, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.ReorderLevel : 0))
                .ForMember(dest => dest.PricingHistory, opt => opt.MapFrom(src => src.PricingHistory
                    .OrderByDescending(p => p.EffectiveFrom)
                    .Take(10)
                    .Select(p => new PricingHistoryDto
                    {
                        PointsCost = p.PointsCost,
                        EffectiveDate = p.EffectiveFrom
                    })));

            // ProductPricing → PricingHistoryDto
            CreateMap<ProductPricing, PricingHistoryDto>()
                .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveFrom));

            // InventoryItem → InventoryResponseDto
            CreateMap<InventoryItem, InventoryResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

            // CreateProductDto → Product (for reference - use Product.Create() factory method in services)
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.ProductCategory, opt => opt.Ignore())
                .ForMember(dest => dest.PricingHistory, opt => opt.Ignore())
                .ForMember(dest => dest.Inventory, opt => opt.Ignore())
                .ForMember(dest => dest.Redemptions, opt => opt.Ignore());
        }
    }
}

using ttb_stock_service.Models.Common;
using ttb_stock_service.Models.DTOs;

namespace ttb_stock_service.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllAsync(ProductQueryParameters query);
    Task<ProductDto?> GetByIdAsync(int productId);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(int productId, UpdateProductDto dto);
    Task<bool> DeleteAsync(int productId);
}

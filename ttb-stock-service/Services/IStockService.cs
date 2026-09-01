using ttb_stock_service.Models.DTOs;

namespace ttb_stock_service.Services;

public interface IStockService
{
    Task<IEnumerable<StockDto>> GetAllAsync(int? productId = null, bool? active = null);
    Task<StockDto?> GetByIdAsync(int stockId);
    Task<IEnumerable<StockDto>> GetByProductIdAsync(int productId);
    Task<StockDto> CreateAsync(CreateStockDto dto);
    Task<StockDto?> UpdateAsync(int stockId, UpdateStockDto dto);
    Task<StockDto> AdjustStockAsync(AdjustStockDto dto);
    Task<bool> DeleteAsync(int stockId);
    Task<IEnumerable<TransactionLogDto>> CheckoutAsync(List<CheckoutItemDto> items);
    Task<IEnumerable<TransactionLogDto>> GetTransactionLogsAsync(int? productId = null, int? stockId = null);
}

using Microsoft.EntityFrameworkCore;
using ttb_stock_service.Data;
using ttb_stock_service.Models.DTOs;
using ttb_stock_service.Models.Entities;

namespace ttb_stock_service.Services;

public class StockService : IStockService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StockService> _logger;

    public StockService(ApplicationDbContext context, ILogger<StockService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<StockDto>> GetAllAsync(int? productId = null, bool? active = null)
    {
        var query = _context.Stocks
            .Include(s => s.Product)
            .AsNoTracking()
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(s => s.ProductId == productId.Value);
        }

        if (active.HasValue)
        {
            query = query.Where(s => s.Active == active.Value);
        }

        var stocks = await query.OrderBy(s => s.StockId).ToListAsync();
        return stocks.Select(MapToDto);
    }

    public async Task<StockDto?> GetByIdAsync(int stockId)
    {
        var stock = await _context.Stocks
            .Include(s => s.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StockId == stockId);

        return stock == null ? null : MapToDto(stock);
    }

    public async Task<IEnumerable<StockDto>> GetByProductIdAsync(int productId)
    {
        var stocks = await _context.Stocks
            .Include(s => s.Product)
            .AsNoTracking()
            .Where(s => s.ProductId == productId)
            .OrderBy(s => s.StockId)
            .ToListAsync();

        return stocks.Select(MapToDto);
    }

    public async Task<StockDto> CreateAsync(CreateStockDto dto)
    {
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID '{dto.ProductId}' was not found.");
        }

        var stock = new Stock
        {
            ProductId = dto.ProductId,
            Amount = dto.Amount,
            Active = dto.Active,
            CreateBy = string.IsNullOrWhiteSpace(dto.CreateBy) ? "SYSTEM" : dto.CreateBy.Trim(),
            UpdateBy = string.IsNullOrWhiteSpace(dto.CreateBy) ? "SYSTEM" : dto.CreateBy.Trim(),
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow
        };

        await _context.Stocks.AddAsync(stock);
        await _context.SaveChangesAsync();

        stock.Product = product;

        _logger.LogInformation("Stock created successfully with StockId: {StockId} for ProductId: {ProductId}", stock.StockId, stock.ProductId);
        return MapToDto(stock);
    }

    public async Task<StockDto?> UpdateAsync(int stockId, UpdateStockDto dto)
    {
        var stock = await _context.Stocks
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.StockId == stockId);

        if (stock == null)
            return null;

        if (dto.Amount.HasValue)
        {
            stock.Amount = dto.Amount.Value;
        }

        if (dto.Active.HasValue)
        {
            stock.Active = dto.Active.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.UpdateBy))
        {
            stock.UpdateBy = dto.UpdateBy.Trim();
        }

        stock.UpdateDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Stock updated successfully with StockId: {StockId}", stock.StockId);
        return MapToDto(stock);
    }

    public async Task<StockDto> AdjustStockAsync(AdjustStockDto dto)
    {
        Stock? stock = null;

        if (dto.StockId.HasValue)
        {
            stock = await _context.Stocks
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.StockId == dto.StockId.Value);
        }
        else
        {
            stock = await _context.Stocks
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.Active);
        }

        if (stock == null)
        {
            var productExists = await _context.Products.AnyAsync(p => p.ProductId == dto.ProductId);
            if (!productExists)
            {
                throw new KeyNotFoundException($"Product with ID '{dto.ProductId}' was not found.");
            }

            if (dto.Operation == StockOperation.Add || dto.Operation == StockOperation.Set)
            {
                stock = new Stock
                {
                    ProductId = dto.ProductId,
                    Amount = dto.Amount,
                    Active = true,
                    CreateBy = string.IsNullOrWhiteSpace(dto.UpdateBy) ? "SYSTEM" : dto.UpdateBy.Trim(),
                    UpdateBy = string.IsNullOrWhiteSpace(dto.UpdateBy) ? "SYSTEM" : dto.UpdateBy.Trim(),
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };

                await _context.Stocks.AddAsync(stock);
                await _context.SaveChangesAsync();

                stock.Product = await _context.Products.FindAsync(dto.ProductId);
                return MapToDto(stock);
            }

            throw new KeyNotFoundException($"No active stock record found for Product ID '{dto.ProductId}'.");
        }

        var currentAmount = stock.Amount ?? 0;

        switch (dto.Operation)
        {
            case StockOperation.Add:
                stock.Amount = currentAmount + dto.Amount;
                break;

            case StockOperation.Deduct:
                if (currentAmount < dto.Amount)
                {
                    throw new InvalidOperationException($"Cannot deduct {dto.Amount}. Current stock amount is only {currentAmount}.");
                }
                stock.Amount = currentAmount - dto.Amount;
                break;

            case StockOperation.Set:
                stock.Amount = dto.Amount;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(dto.Operation), "Invalid stock operation.");
        }

        if (!string.IsNullOrWhiteSpace(dto.UpdateBy))
        {
            stock.UpdateBy = dto.UpdateBy.Trim();
        }

        stock.UpdateDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Stock adjusted: ProductId {ProductId}, StockId {StockId}, Operation {Op}, Amount {Amount}",
            dto.ProductId, stock.StockId, dto.Operation, dto.Amount);

        return MapToDto(stock);
    }

    public async Task<bool> DeleteAsync(int stockId)
    {
        var stock = await _context.Stocks.FindAsync(stockId);
        if (stock == null)
            return false;

        _context.Stocks.Remove(stock);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Stock deleted successfully with StockId: {StockId}", stockId);
        return true;
    }

    public async Task<IEnumerable<TransactionLogDto>> CheckoutAsync(List<CheckoutItemDto> items)
    {
        if (items == null || items.Count == 0)
        {
            throw new ArgumentException("Checkout request list cannot be empty.");
        }

        // 1. Basic validation of request items
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item == null)
            {
                throw new ArgumentException($"Item at index {i} cannot be null.");
            }

            if (item.ProductId <= 0)
            {
                throw new ArgumentException($"Invalid ProductId '{item.ProductId}' at item {i + 1}. Product ID must be greater than 0.");
            }

            if (item.Amount <= 0)
            {
                throw new ArgumentException($"Invalid Amount '{item.Amount}' for Product ID '{item.ProductId}'. Amount must be greater than 0.");
            }
        }

        // 2. Aggregate requested amount by ProductId
        var groupedRequests = items
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalRequestedAmount = g.Sum(x => x.Amount)
            })
            .ToList();

        var requestedProductIds = groupedRequests.Select(g => g.ProductId).ToList();

        // 3. Validate product existence and active status
        var products = await _context.Products
            .Where(p => requestedProductIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId);

        foreach (var req in groupedRequests)
        {
            if (!products.TryGetValue(req.ProductId, out var product))
            {
                throw new KeyNotFoundException($"Product with ID '{req.ProductId}' was not found.");
            }

            if (!product.Active)
            {
                throw new InvalidOperationException($"Product '{product.ProductName}' (ID: {product.ProductId}) is inactive.");
            }
        }

        // 4. Fetch and validate active stocks with available quantities
        var availableStocks = await _context.Stocks
            .Where(s => requestedProductIds.Contains(s.ProductId!.Value) && s.Active && (s.Amount ?? 0) > 0)
            .OrderBy(s => s.CreateDate)
            .ThenBy(s => s.StockId)
            .ToListAsync();

        var stocksByProduct = availableStocks
            .GroupBy(s => s.ProductId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var req in groupedRequests)
        {
            var product = products[req.ProductId];
            stocksByProduct.TryGetValue(req.ProductId, out var stockList);
            var totalAvailable = stockList?.Sum(s => s.Amount ?? 0) ?? 0;

            if (totalAvailable < req.TotalRequestedAmount)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.ProductName}' (ID: {product.ProductId}). " +
                    $"Requested: {req.TotalRequestedAmount}, Available: {totalAvailable}.");
            }
        }

        // 5. Execute deduction from oldest stocks (FIFO) and record transaction logs
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var logs = new List<TransactionLog>();

            foreach (var item in items)
            {
                var createdBy = string.IsNullOrWhiteSpace(item.CreateBy) ? "SYSTEM" : item.CreateBy.Trim();
                var remainingToDeduct = item.Amount;
                var product = products[item.ProductId];

                if (!stocksByProduct.TryGetValue(item.ProductId, out var productStockList))
                {
                    continue;
                }

                foreach (var stock in productStockList)
                {
                    if (remainingToDeduct <= 0)
                        break;

                    var currentStockAmount = stock.Amount ?? 0;
                    if (currentStockAmount <= 0)
                        continue;

                    var deductAmount = Math.Min(currentStockAmount, remainingToDeduct);
                    stock.Amount = currentStockAmount - deductAmount;
                    stock.UpdateDate = DateTime.UtcNow;
                    stock.UpdateBy = createdBy;
                    remainingToDeduct -= deductAmount;

                    var log = new TransactionLog
                    {
                        StockId = stock.StockId,
                        ProductId = item.ProductId,
                        Amount = deductAmount,
                        CreateDate = DateTime.UtcNow,
                        CreateBy = createdBy,
                        Product = product,
                        Stock = stock
                    };

                    await _context.TransactionLogs.AddAsync(log);
                    logs.Add(log);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Stock checkout succeeded for {ItemCount} items, generated {LogCount} transaction logs.",
                items.Count, logs.Count);

            return logs.Select(MapToTransactionLogDto).ToList();
        });
    }

    public async Task<IEnumerable<TransactionLogDto>> GetTransactionLogsAsync(int? productId = null, int? stockId = null)
    {
        var query = _context.TransactionLogs
            .Include(t => t.Product)
            .Include(t => t.Stock)
            .AsNoTracking()
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(t => t.ProductId == productId.Value);
        }

        if (stockId.HasValue)
        {
            query = query.Where(t => t.StockId == stockId.Value);
        }

        var logs = await query
            .OrderByDescending(t => t.CreateDate)
            .ThenByDescending(t => t.TransactionId)
            .ToListAsync();

        return logs.Select(MapToTransactionLogDto);
    }

    private static StockDto MapToDto(Stock s)
    {
        return new StockDto
        {
            StockId = s.StockId,
            ProductId = s.ProductId,
            ProductCode = s.Product?.ProductCode,
            ProductName = s.Product?.ProductName,
            Amount = s.Amount,
            Active = s.Active,
            CreateDate = s.CreateDate,
            CreateBy = s.CreateBy,
            UpdateDate = s.UpdateDate,
            UpdateBy = s.UpdateBy
        };
    }

    private static TransactionLogDto MapToTransactionLogDto(TransactionLog t)
    {
        return new TransactionLogDto
        {
            TransactionId = t.TransactionId,
            StockId = t.StockId,
            ProductId = t.ProductId,
            ProductCode = t.Product?.ProductCode,
            ProductName = t.Product?.ProductName,
            Amount = t.Amount,
            CreateDate = t.CreateDate,
            CreateBy = t.CreateBy
        };
    }
}

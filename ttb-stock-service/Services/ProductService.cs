using Microsoft.EntityFrameworkCore;
using ttb_stock_service.Data;
using ttb_stock_service.Models.Common;
using ttb_stock_service.Models.DTOs;
using ttb_stock_service.Models.Entities;

namespace ttb_stock_service.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(ProductQueryParameters query)
    {
        var dbQuery = _context.Products
            .Include(p => p.Stocks)
            .AsNoTracking()
            .AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchLower = query.Search.Trim().ToLower();
            dbQuery = dbQuery.Where(p => p.ProductName.ToLower().Contains(searchLower) || p.ProductCode.ToLower().Contains(searchLower));
        }

        if (query.Active.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.Active == query.Active.Value);
        }

        // Total count
        var totalCount = await dbQuery.CountAsync();

        // Sorting
        dbQuery = (query.SortBy?.ToLower(), query.IsAscending) switch
        {
            ("productname", true) => dbQuery.OrderBy(p => p.ProductName),
            ("productname", false) => dbQuery.OrderByDescending(p => p.ProductName),
            ("productcode", true) => dbQuery.OrderBy(p => p.ProductCode),
            ("productcode", false) => dbQuery.OrderByDescending(p => p.ProductCode),
            ("productprice", true) => dbQuery.OrderBy(p => p.ProductPrice),
            ("productprice", false) => dbQuery.OrderByDescending(p => p.ProductPrice),
            ("createdate", true) => dbQuery.OrderBy(p => p.CreateDate),
            _ => dbQuery.OrderByDescending(p => p.CreateDate)
        };

        // Pagination
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var items = await dbQuery
            .Where(p => p.Active == true) // Only include active products in the paginated result
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return new PagedResult<ProductDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<ProductDto?> GetByIdAsync(int productId)
    {
        var product = await _context.Products
            .Include(p => p.Stocks)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var codeTrim = dto.ProductCode.Trim();
        var nameTrim = dto.ProductName.Trim();

        // Check for duplicate PRODUCT_CODE
        var codeExists = await _context.Products.AnyAsync(p => p.ProductCode == codeTrim);
        if (codeExists)
        {
            throw new InvalidOperationException($"Product with code '{codeTrim}' already exists.");
        }

        // Check for duplicate PRODUCT_NAME
        var nameExists = await _context.Products.AnyAsync(p => p.ProductName == nameTrim);
        if (nameExists)
        {
            throw new InvalidOperationException($"Product with name '{nameTrim}' already exists.");
        }

        var product = new Product
        {
            ProductCode = codeTrim,
            ProductName = nameTrim,
            ProductPrice = dto.ProductPrice,
            Active = dto.Active,
            CreateBy = string.IsNullOrWhiteSpace(dto.CreateBy) ? "SYSTEM" : dto.CreateBy.Trim(),
            UpdateBy = string.IsNullOrWhiteSpace(dto.CreateBy) ? "SYSTEM" : dto.CreateBy.Trim(),
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow
        };

        if (dto.InitialAmount.HasValue && dto.InitialAmount.Value > 0)
        {
            var initialStock = new Stock
            {
                Amount = dto.InitialAmount.Value,
                Active = true,
                CreateBy = product.CreateBy,
                UpdateBy = product.UpdateBy,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            product.Stocks.Add(initialStock);
        }

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product created successfully with ProductId: {ProductId}, Code: {ProductCode}", product.ProductId, product.ProductCode);

        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(int productId, UpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
            return null;

        // Check if PRODUCT_CODE is changing and conflicts
        if (!string.IsNullOrWhiteSpace(dto.ProductCode))
        {
            var codeTrim = dto.ProductCode.Trim();
            if (codeTrim != product.ProductCode)
            {
                var codeExists = await _context.Products.AnyAsync(p => p.ProductCode == codeTrim && p.ProductId != productId);
                if (codeExists)
                {
                    throw new InvalidOperationException($"Product with code '{codeTrim}' already exists.");
                }
                product.ProductCode = codeTrim;
            }
        }

        // Check if PRODUCT_NAME is changing and conflicts
        if (!string.IsNullOrWhiteSpace(dto.ProductName))
        {
            var nameTrim = dto.ProductName.Trim();
            if (nameTrim != product.ProductName)
            {
                var nameExists = await _context.Products.AnyAsync(p => p.ProductName == nameTrim && p.ProductId != productId);
                if (nameExists)
                {
                    throw new InvalidOperationException($"Product with name '{nameTrim}' already exists.");
                }
                product.ProductName = nameTrim;
            }
        }

        if (dto.ProductPrice.HasValue) product.ProductPrice = dto.ProductPrice.Value;
        if (dto.Active.HasValue) product.Active = dto.Active.Value;
        if (!string.IsNullOrWhiteSpace(dto.UpdateBy)) product.UpdateBy = dto.UpdateBy.Trim();

        product.UpdateDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Product updated successfully with ProductId: {ProductId}", product.ProductId);

        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(int productId)
    {
        var product = await _context.Products
            .Include(p => p.Stocks)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product deleted successfully with ProductId: {ProductId}", productId);
        return true;
    }

    private static ProductDto MapToDto(Product p)
    {
        var stockList = p.Stocks?.Select(s => new StockSummaryDto
        {
            StockId = s.StockId,
            Amount = s.Amount,
            Active = s.Active,
            CreateDate = s.CreateDate,
            CreateBy = s.CreateBy,
            UpdateDate = s.UpdateDate,
            UpdateBy = s.UpdateBy
        }).ToList() ?? new List<StockSummaryDto>();

        return new ProductDto
        {
            ProductId = p.ProductId,
            ProductCode = p.ProductCode,
            ProductName = p.ProductName,
            ProductPrice = p.ProductPrice,
            Active = p.Active,
            CreateDate = p.CreateDate,
            CreateBy = p.CreateBy,
            UpdateDate = p.UpdateDate,
            UpdateBy = p.UpdateBy,
            TotalStockAmount = stockList.Where(s => s.Active).Sum(s => s.Amount ?? 0),
            Stocks = stockList
        };
    }
}

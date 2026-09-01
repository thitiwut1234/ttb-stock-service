using Microsoft.AspNetCore.Mvc;
using ttb_stock_service.Models.Common;
using ttb_stock_service.Models.DTOs;
using ttb_stock_service.Services;

namespace ttb_stock_service.Controllers;

public class StocksController : BaseApiController
{
    private readonly IStockService _stockService;
    private readonly ILogger<StocksController> _logger;

    public StocksController(IStockService stockService, ILogger<StocksController> logger)
    {
        _stockService = stockService;
        _logger = logger;
    }

    /// <summary>
    /// Get all stock records with optional product or active status filter
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StockDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockDto>>>> GetAll([FromQuery] int? productId, [FromQuery] bool? active)
    {
        var stocks = await _stockService.GetAllAsync(productId, active);
        return Success(stocks, "Stocks retrieved successfully");
    }

    /// <summary>
    /// Get stock by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StockDto>>> GetById(int id)
    {
        var stock = await _stockService.GetByIdAsync(id);
        if (stock == null)
        {
            return NotFoundError<StockDto>($"Stock with ID '{id}' was not found.");
        }

        return Success(stock, "Stock retrieved successfully");
    }

    /// <summary>
    /// Get all stock entries for a specific product
    /// </summary>
    [HttpGet("product/{productId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StockDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockDto>>>> GetByProductId(int productId)
    {
        var stocks = await _stockService.GetByProductIdAsync(productId);
        return Success(stocks, $"Stocks for product '{productId}' retrieved successfully");
    }

    /// <summary>
    /// Create new stock entry for a product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StockDto>>> Create([FromBody] CreateStockDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequestError<StockDto>("Validation failed", errors);
        }

        var created = await _stockService.CreateAsync(dto);
        return CreatedSuccess($"/api/stocks/{created.StockId}", created, "Stock created successfully");
    }

    /// <summary>
    /// Update stock entry
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StockDto>>> Update(int id, [FromBody] UpdateStockDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequestError<StockDto>("Validation failed", errors);
        }

        var updated = await _stockService.UpdateAsync(id, dto);
        if (updated == null)
        {
            return NotFoundError<StockDto>($"Stock with ID '{id}' was not found.");
        }

        return Success(updated, "Stock updated successfully");
    }

    /// <summary>
    /// Adjust stock quantity (Operation: 1=Add, 2=Deduct, 3=Set)
    /// </summary>
    [HttpPost("adjust")]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StockDto>>> AdjustStock([FromBody] AdjustStockDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequestError<StockDto>("Validation failed", errors);
        }

        var result = await _stockService.AdjustStockAsync(dto);
        return Success(result, $"Stock adjusted successfully ({dto.Operation}: {dto.Amount})");
    }

    /// <summary>
    /// Delete a stock entry
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        var deleted = await _stockService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFoundError($"Stock with ID '{id}' was not found.");
        }

        return Success($"Stock with ID '{id}' was deleted successfully");
    }

    /// <summary>
    /// Checkout items, deduct from oldest available stock (FIFO), and log transactions
    /// </summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionLogDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionLogDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionLogDto>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionLogDto>>>> Checkout([FromBody] List<CheckoutItemDto> items)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequestError<IEnumerable<TransactionLogDto>>("Validation failed", errors);
        }

        var result = await _stockService.CheckoutAsync(items);
        return Success(result, "Checkout processed successfully");
    }

    /// <summary>
    /// Get transaction logs with optional product or stock filter
    /// </summary>
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionLogDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionLogDto>>>> GetTransactions([FromQuery] int? productId, [FromQuery] int? stockId)
    {
        var logs = await _stockService.GetTransactionLogsAsync(productId, stockId);
        return Success(logs, "Transaction logs retrieved successfully");
    }
}

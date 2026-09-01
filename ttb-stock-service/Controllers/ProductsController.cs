using Microsoft.AspNetCore.Mvc;
using ttb_stock_service.Models.Common;
using ttb_stock_service.Models.DTOs;
using ttb_stock_service.Services;

namespace ttb_stock_service.Controllers;

public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of products with pagination, search, and filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetAll([FromQuery] ProductQueryParameters query)
    {
        var result = await _productService.GetAllAsync(query);
        return Success(result, "Products retrieved successfully");
    }

    /// <summary>
    /// Get product by ID including its stock details
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFoundError<ProductDto>($"Product with ID '{id}' was not found.");
        }

        return Success(product, "Product retrieved successfully");
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequestError<ProductDto>("Validation failed", errors);
        }

        var created = await _productService.CreateAsync(dto);
        return CreatedSuccess($"/api/products/{created.ProductId}", created, "Product created successfully");
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequestError<ProductDto>("Validation failed", errors);
        }

        var updated = await _productService.UpdateAsync(id, dto);
        if (updated == null)
        {
            return NotFoundError<ProductDto>($"Product with ID '{id}' was not found.");
        }

        return Success(updated, "Product updated successfully");
    }

    /// <summary>
    /// Delete a product and its associated stocks
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFoundError($"Product with ID '{id}' was not found.");
        }

        return Success($"Product with ID '{id}' was deleted successfully");
    }
}

using System.ComponentModel.DataAnnotations;

namespace ttb_stock_service.Models.DTOs;

public class CreateProductDto
{
    [Required(ErrorMessage = "ProductCode is required")]
    [MaxLength(50, ErrorMessage = "ProductCode cannot exceed 50 characters")]
    public string ProductCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "ProductName is required")]
    [MaxLength(255, ErrorMessage = "ProductName cannot exceed 255 characters")]
    public string ProductName { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "ProductPrice must be greater than or equal to 0")]
    public int? ProductPrice { get; set; }

    public bool Active { get; set; } = true;

    [MaxLength(255)]
    public string? CreateBy { get; set; } = "SYSTEM";

    // Optional initial stock amount
    public int? InitialAmount { get; set; }
}

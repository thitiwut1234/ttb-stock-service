using System.ComponentModel.DataAnnotations;

namespace ttb_stock_service.Models.DTOs;

public class UpdateProductDto
{
    [MaxLength(50, ErrorMessage = "ProductCode cannot exceed 50 characters")]
    public string? ProductCode { get; set; }

    [MaxLength(255, ErrorMessage = "ProductName cannot exceed 255 characters")]
    public string? ProductName { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "ProductPrice must be greater than or equal to 0")]
    public int? ProductPrice { get; set; }

    public bool? Active { get; set; }

    [MaxLength(255)]
    public string? UpdateBy { get; set; } = "SYSTEM";
}

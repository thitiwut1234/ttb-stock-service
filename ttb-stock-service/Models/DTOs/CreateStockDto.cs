using System.ComponentModel.DataAnnotations;

namespace ttb_stock_service.Models.DTOs;

public class CreateStockDto
{
    [Required(ErrorMessage = "ProductId is required")]
    public int ProductId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Amount cannot be negative")]
    public int Amount { get; set; } = 0;

    public bool Active { get; set; } = true;

    [MaxLength(255)]
    public string? CreateBy { get; set; } = "SYSTEM";
}

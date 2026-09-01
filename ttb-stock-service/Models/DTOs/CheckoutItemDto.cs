using System.ComponentModel.DataAnnotations;

namespace ttb_stock_service.Models.DTOs;

public class CheckoutItemDto
{
    [Required(ErrorMessage = "ProductId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public int Amount { get; set; }

    [MaxLength(255)]
    public string? CreateBy { get; set; } = "SYSTEM";
}


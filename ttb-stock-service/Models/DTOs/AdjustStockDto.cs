using System.ComponentModel.DataAnnotations;

namespace ttb_stock_service.Models.DTOs;

public enum StockOperation
{
    Add = 1,
    Deduct = 2,
    Set = 3
}

public class AdjustStockDto
{
    [Required(ErrorMessage = "ProductId is required")]
    public int ProductId { get; set; }

    public int? StockId { get; set; }

    [Required(ErrorMessage = "Operation is required (1=Add, 2=Deduct, 3=Set)")]
    public StockOperation Operation { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Amount must be greater than or equal to 0")]
    public int Amount { get; set; }

    [MaxLength(255)]
    public string? UpdateBy { get; set; } = "SYSTEM";
}

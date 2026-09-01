using System.ComponentModel.DataAnnotations;

namespace ttb_stock_service.Models.DTOs;

public class UpdateStockDto
{
    [Range(0, int.MaxValue, ErrorMessage = "Amount cannot be negative")]
    public int? Amount { get; set; }

    public bool? Active { get; set; }

    [MaxLength(255)]
    public string? UpdateBy { get; set; } = "SYSTEM";
}

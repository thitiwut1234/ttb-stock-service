namespace ttb_stock_service.Models.DTOs;

public class TransactionLogDto
{
    public int TransactionId { get; set; }
    public int? StockId { get; set; }
    public int? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int? Amount { get; set; }
    public DateTime CreateDate { get; set; }
    public string CreateBy { get; set; } = "SYSTEM";
}


namespace ttb_stock_service.Models.DTOs;

public class StockDto
{
    public int StockId { get; set; }
    public int? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int? Amount { get; set; }
    public bool Active { get; set; }
    public DateTime CreateDate { get; set; }
    public string CreateBy { get; set; } = string.Empty;
    public DateTime? UpdateDate { get; set; }
    public string? UpdateBy { get; set; }
}

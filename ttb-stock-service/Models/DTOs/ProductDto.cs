namespace ttb_stock_service.Models.DTOs;

public class ProductDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int? ProductPrice { get; set; }
    public bool Active { get; set; }
    public DateTime CreateDate { get; set; }
    public string CreateBy { get; set; } = string.Empty;
    public DateTime? UpdateDate { get; set; }
    public string? UpdateBy { get; set; }
    public int TotalStockAmount { get; set; }
    public List<StockSummaryDto> Stocks { get; set; } = new();
}

public class StockSummaryDto
{
    public int StockId { get; set; }
    public int? Amount { get; set; }
    public bool Active { get; set; }
    public DateTime CreateDate { get; set; }
    public string CreateBy { get; set; } = string.Empty;
    public DateTime? UpdateDate { get; set; }
    public string? UpdateBy { get; set; }
}

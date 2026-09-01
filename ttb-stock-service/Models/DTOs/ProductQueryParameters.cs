namespace ttb_stock_service.Models.DTOs;

public class ProductQueryParameters
{
    public string? Search { get; set; }
    public bool? Active { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreateDate";
    public bool IsAscending { get; set; } = false;
}

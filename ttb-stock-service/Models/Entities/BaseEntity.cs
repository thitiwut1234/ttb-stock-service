namespace ttb_stock_service.Models.Entities;

public abstract class BaseEntity
{
    public bool Active { get; set; } = true;
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    public string CreateBy { get; set; } = "SYSTEM";
    public DateTime? UpdateDate { get; set; }
    public string? UpdateBy { get; set; } = "SYSTEM";
}

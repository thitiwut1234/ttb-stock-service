using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ttb_stock_service.Models.Entities;

[Table("transaction_log")]
public class TransactionLog
{
    [Key]
    [Column("transaction_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TransactionId { get; set; }

    [Column("stock_id")]
    public int? StockId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("amount")]
    public int? Amount { get; set; }

    [Column("create_date")]
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    [Column("create_by")]
    [MaxLength(255)]
    public string CreateBy { get; set; } = "SYSTEM";

    // Navigation properties
    [ForeignKey(nameof(StockId))]
    public virtual Stock? Stock { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
}


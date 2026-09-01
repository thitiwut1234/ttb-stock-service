using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ttb_stock_service.Models.Entities;

[Table("stock")]
public class Stock : BaseEntity
{
    [Key]
    [Column("stock_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StockId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("amount")]
    public int? Amount { get; set; } = 0;

    // Navigation property
    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }

    public virtual ICollection<TransactionLog> TransactionLogs { get; set; } = new List<TransactionLog>();
}

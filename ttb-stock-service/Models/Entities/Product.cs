using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ttb_stock_service.Models.Entities;

[Table("product")]
public class Product : BaseEntity
{
    [Key]
    [Column("product_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }

    [Column("product_code")]
    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    [Column("product_name")]
    [Required]
    [MaxLength(255)]
    public string ProductName { get; set; } = string.Empty;

    [Column("product_price")]
    public int? ProductPrice { get; set; }

    // Navigation property for 1-to-many relationship
    public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    public virtual ICollection<TransactionLog> TransactionLogs { get; set; } = new List<TransactionLog>();
}

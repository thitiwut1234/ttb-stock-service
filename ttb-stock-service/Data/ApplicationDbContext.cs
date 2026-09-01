using Microsoft.EntityFrameworkCore;
using ttb_stock_service.Models.Entities;

namespace ttb_stock_service.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<TransactionLog> TransactionLogs => Set<TransactionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map PRODUCT Table
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("product");
            
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId)
                  .HasColumnName("product_id")
                  .UseIdentityAlwaysColumn();

            entity.Property(e => e.ProductCode)
                  .HasColumnName("product_code")
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.ProductName)
                  .HasColumnName("product_name")
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.ProductPrice)
                  .HasColumnName("product_price");

            entity.Property(e => e.Active)
                  .HasColumnName("active")
                  .HasDefaultValue(true);

            entity.Property(e => e.CreateDate)
                  .HasColumnName("create_date")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreateBy)
                  .HasColumnName("create_by")
                  .HasMaxLength(255)
                  .HasDefaultValue("SYSTEM");

            entity.Property(e => e.UpdateDate)
                  .HasColumnName("update_date")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdateBy)
                  .HasColumnName("update_by")
                  .HasMaxLength(255)
                  .HasDefaultValue("SYSTEM");

            // Constraints
            entity.HasIndex(e => e.ProductCode).IsUnique();
            entity.HasIndex(e => e.ProductName).IsUnique();
        });

        // Map STOCK Table
        modelBuilder.Entity<Stock>(entity =>
        {
            entity.ToTable("stock");

            entity.HasKey(e => e.StockId);
            entity.Property(e => e.StockId)
                  .HasColumnName("stock_id")
                  .UseIdentityAlwaysColumn();

            entity.Property(e => e.ProductId)
                  .HasColumnName("product_id");

            entity.Property(e => e.Amount)
                  .HasColumnName("amount");

            entity.Property(e => e.Active)
                  .HasColumnName("active")
                  .HasDefaultValue(true);

            entity.Property(e => e.CreateDate)
                  .HasColumnName("create_date")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreateBy)
                  .HasColumnName("create_by")
                  .HasMaxLength(255)
                  .HasDefaultValue("SYSTEM");

            entity.Property(e => e.UpdateDate)
                  .HasColumnName("update_date")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdateBy)
                  .HasColumnName("update_by")
                  .HasMaxLength(255)
                  .HasDefaultValue("SYSTEM");

            // Define Foreign Key Relationship: PRODUCT (1) -> (N) STOCK with PRODUCT_ID
            entity.HasOne(s => s.Product)
                  .WithMany(p => p.Stocks)
                  .HasForeignKey(s => s.ProductId)
                  .HasConstraintName("fk_product")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(s => s.ProductId);
        });

        // Map TRANSACTION_LOG Table
        modelBuilder.Entity<TransactionLog>(entity =>
        {
            entity.ToTable("transaction_log");

            entity.HasKey(e => e.TransactionId);
            entity.Property(e => e.TransactionId)
                  .HasColumnName("transaction_id")
                  .UseIdentityAlwaysColumn();

            entity.Property(e => e.StockId)
                  .HasColumnName("stock_id");

            entity.Property(e => e.ProductId)
                  .HasColumnName("product_id");

            entity.Property(e => e.Amount)
                  .HasColumnName("amount");

            entity.Property(e => e.CreateDate)
                  .HasColumnName("create_date")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreateBy)
                  .HasColumnName("create_by")
                  .HasMaxLength(255)
                  .HasDefaultValue("SYSTEM");

            // Define Foreign Key Relationships
            entity.HasOne(t => t.Product)
                  .WithMany(p => p.TransactionLogs)
                  .HasForeignKey(t => t.ProductId)
                  .HasConstraintName("fk_transaction_log_product")
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Stock)
                  .WithMany(s => s.TransactionLogs)
                  .HasForeignKey(t => t.StockId)
                  .HasConstraintName("fk_transaction_log_stock")
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(t => t.ProductId);
            entity.HasIndex(t => t.StockId);
            entity.HasIndex(t => t.CreateDate);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreateDate == default)
                    entry.Entity.CreateDate = now;

                if (string.IsNullOrWhiteSpace(entry.Entity.CreateBy))
                    entry.Entity.CreateBy = "SYSTEM";

                if (entry.Entity.UpdateDate == null || entry.Entity.UpdateDate == default)
                    entry.Entity.UpdateDate = now;

                if (string.IsNullOrWhiteSpace(entry.Entity.UpdateBy))
                    entry.Entity.UpdateBy = "SYSTEM";
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdateDate = now;
                if (string.IsNullOrWhiteSpace(entry.Entity.UpdateBy))
                    entry.Entity.UpdateBy = "SYSTEM";
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

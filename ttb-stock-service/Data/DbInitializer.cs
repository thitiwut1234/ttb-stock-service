using Microsoft.EntityFrameworkCore;
using ttb_stock_service.Models.Entities;

namespace ttb_stock_service.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            await context.Database.EnsureCreatedAsync();

            if (await context.Products.AnyAsync())
            {
                return; // DB has already been seeded
            }

            logger.LogInformation("Seeding initial Product and Stock data...");

            var product1 = new Product
            {
                ProductCode = "PRD-LAPTOP-001",
                ProductName = "ThinkPad X1 Carbon Gen 11",
                ProductPrice = 65000,
                Active = true,
                CreateBy = "SYSTEM",
                UpdateBy = "SYSTEM"
            };

            var product2 = new Product
            {
                ProductCode = "PRD-MOUSE-001",
                ProductName = "Logitech MX Master 3S",
                ProductPrice = 3500,
                Active = true,
                CreateBy = "SYSTEM",
                UpdateBy = "SYSTEM"
            };

            var product3 = new Product
            {
                ProductCode = "PRD-KEYBOARD-001",
                ProductName = "Keychron Q1 Pro Mechanical Keyboard",
                ProductPrice = 7200,
                Active = true,
                CreateBy = "SYSTEM",
                UpdateBy = "SYSTEM"
            };

            await context.Products.AddRangeAsync(product1, product2, product3);
            await context.SaveChangesAsync();

            var stock1 = new Stock
            {
                ProductId = product1.ProductId,
                Amount = 50,
                Active = true,
                CreateBy = "SYSTEM",
                UpdateBy = "SYSTEM"
            };

            var stock2 = new Stock
            {
                ProductId = product2.ProductId,
                Amount = 150,
                Active = true,
                CreateBy = "SYSTEM",
                UpdateBy = "SYSTEM"
            };

            var stock3 = new Stock
            {
                ProductId = product3.ProductId,
                Amount = 80,
                Active = true,
                CreateBy = "SYSTEM",
                UpdateBy = "SYSTEM"
            };

            await context.Stocks.AddRangeAsync(stock1, stock2, stock3);
            await context.SaveChangesAsync();

            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing/seeding the database.");
        }
    }
}

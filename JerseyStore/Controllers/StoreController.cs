using Dapper;
using JerseyStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace JerseyStore.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StoreController(IConfiguration configuration, ILogger<StoreController> logger) : ControllerBase
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            await using var db = new SqlConnection(_connectionString);
            var products = await db.QueryAsync<Product>(
                "SELECT Id, TeamName, Price, Stock FROM Products ORDER BY Id");
            return Ok(products);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Unable to load products");
            return StatusCode(503, new { success = false, message = "Products are temporarily unavailable." });
        }
    }

    [HttpPost("order")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
    {
        if (request.Items is null || request.Items.Count == 0 || request.Items.Any(item => item is null))
            return BadRequest(new { success = false, message = "Your cart must contain valid items." });

        try
        {
            await using var db = new SqlConnection(_connectionString);
            await db.OpenAsync();
            // Disposal rolls back every uncommitted transaction, including early returns.
            using var transaction = db.BeginTransaction(IsolationLevel.ReadCommitted);
            decimal totalAmount = 0;
            var lines = new List<(OrderItem Item, decimal UnitPrice)>();

            // Consistent lock ordering reduces deadlocks between overlapping carts.
            foreach (var item in request.Items.OrderBy(item => item.ProductId))
            {
                // An atomic conditional update prevents overselling, including duplicate cart lines.
                // OUTPUT captures the exact price used by this transaction.
                const string reserveSql = """
                    UPDATE Products SET Stock = Stock - @Quantity
                    OUTPUT inserted.Price
                    WHERE Id = @ProductId AND Stock >= @Quantity;
                    """;
                var price = await db.QuerySingleOrDefaultAsync<decimal?>(reserveSql,
                    new { item.ProductId, item.Quantity }, transaction);
                if (price is null)
                    return Conflict(new { success = false, message = $"Product {item.ProductId} is unavailable or has insufficient stock." });

                totalAmount += price.Value * item.Quantity;
                lines.Add((item, price.Value));
            }

            var orderNumber = "JS" + Guid.NewGuid().ToString("N");
            var orderDate = DateTime.UtcNow;
            const string orderSql = """
                INSERT INTO Orders (OrderNumber, CustomerName, Address, Phone, OrderDate, TotalAmount)
                VALUES (@OrderNumber, @CustomerName, @Address, @Phone, @OrderDate, @TotalAmount);
                SELECT CAST(SCOPE_IDENTITY() AS int);
                """;
            var orderId = await db.QuerySingleAsync<int>(orderSql, new
            {
                OrderNumber = orderNumber,
                CustomerName = request.CustomerName.Trim(),
                Address = request.Address.Trim(),
                Phone = request.Phone.Trim(),
                OrderDate = orderDate,
                TotalAmount = totalAmount
            }, transaction);

            const string itemSql = """
                INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, JerseyNumber, PrintName)
                VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @JerseyNumber, @PrintName);
                """;
            foreach (var (item, unitPrice) in lines)
                await db.ExecuteAsync(itemSql, new
                {
                    OrderId = orderId, item.ProductId, item.Quantity,
                    UnitPrice = unitPrice, item.JerseyNumber,
                    PrintName = item.PrintName?.Trim()
                }, transaction);

            transaction.Commit();
            return Ok(new { success = true, message = "Order created successfully.", orderDate, totalAmount, orderNumber });
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Unable to create order");
            return StatusCode(503, new { success = false, message = "Orders are temporarily unavailable. Please try again later." });
        }
    }
}

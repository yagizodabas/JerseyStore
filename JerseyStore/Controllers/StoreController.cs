using Dapper;
using JerseyStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace JerseyStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly string _connectionString;

        public StoreController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    string sqlQuery = "SELECT Id, TeamName, Price, Stock FROM Products";

                    var products = await db.QueryAsync<Product>(sqlQuery);

                    return Ok(products);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEVELOPER LOG - GET PRODUCTS ERROR] : {ex.Message} | StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "An error occurred while fetching products from the database." });
            }
        }


        [HttpPost("order")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
            {
                return BadRequest(new { success = false, message = "Order data cannot be empty or your cart contains no items." });
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();

                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        decimal totalAmount = 0;

                        foreach (var item in request.Items)
                        {
                            string getProductSql = "SELECT Id, TeamName, Price, Stock FROM Products WHERE Id = @Id";

                            var jersey = await db.QueryFirstOrDefaultAsync<Product>(getProductSql, new { Id = item.ProductId }, transaction);

                            if (jersey == null)
                            {
                                transaction.Rollback();
                                return StatusCode(406, new { success = false, message = $"Order failed: product with ID {item.ProductId} was not found in our system." });
                            }

                            if (jersey.Stock < item.Quantity)
                            {
                                transaction.Rollback();
                                return BadRequest(new { success = false, message = $"Order failed: not enough stock for {jersey.TeamName}. Remaining stock: {jersey.Stock}" });
                            }

                            totalAmount += jersey.Price * item.Quantity;
                        }

                        Random rnd = new Random();
                        string orderNumber = "HGN" + rnd.Next(1000000, 9999999).ToString();
                        DateTime orderDate = DateTime.Now;

                        string insertOrderSql = @"
                            INSERT INTO Orders (OrderNumber, CustomerName, Address, Phone, OrderDate, TotalAmount)
                            VALUES (@OrderNumber, @CustomerName, @Address, @Phone, @OrderDate, @TotalAmount);
                            SELECT CAST(SCOPE_IDENTITY() as int);";

                        int newOrderId = await db.QuerySingleAsync<int>(insertOrderSql, new
                        {
                            OrderNumber = orderNumber,
                            CustomerName = request.CustomerName,
                            Address = request.Address,
                            Phone = request.Phone,
                            OrderDate = orderDate,
                            TotalAmount = totalAmount
                        }, transaction);

                        foreach (var item in request.Items)
                        {
                            string getProductSql = "SELECT Price FROM Products WHERE Id = @Id";
                            decimal unitPrice = await db.QuerySingleAsync<decimal>(getProductSql, new { Id = item.ProductId }, transaction);

                            string insertItemSql = @"
                                INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, JerseyNumber, PrintName)
                                VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @JerseyNumber, @PrintName);";

                            await db.ExecuteAsync(insertItemSql, new
                            {
                                OrderId = newOrderId,
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                UnitPrice = unitPrice,
                                JerseyNumber = item.JerseyNumber,
                                PrintName = item.PrintName
                            }, transaction);

                            string updateStockSql = "UPDATE Products SET Stock = Stock - @Quantity WHERE Id = @Id";
                            await db.ExecuteAsync(updateStockSql, new { Quantity = item.Quantity, Id = item.ProductId }, transaction);
                        }

                        transaction.Commit();

                        var response = new
                        {
                            success = true,
                            message = "Order created successfully.",
                            orderDate = orderDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            totalAmount = totalAmount,
                            orderNumber = orderNumber
                        };

                        return Ok(response);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        Console.WriteLine($"[DEVELOPER LOG - SYSTEM ERROR] : {ex.Message} | StackTrace: {ex.StackTrace}");

                        return StatusCode(500, new
                        {
                            success = false,
                            message = "A system error occurred while processing your order. Please try again later."
                        });
                    }
                }
            }
        }
    }
}
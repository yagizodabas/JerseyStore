using JerseyStore.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace JerseyStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {

        private static readonly List<Product> ProductList = new List<Product>
        {
            new Product { Id = 1, TeamName = "Fenerbahçe", Price = 5990, Stock = 120 },
            new Product { Id = 2, TeamName = "Galatasaray", Price = 5900, Stock = 150 },
            new Product { Id = 3, TeamName = "Beşiktaş", Price = 5800, Stock = 130 },
        };


        [HttpGet("products")]
        public IActionResult GetProducts()
        {
            return Ok(ProductList);
        }


        [HttpPost("order")]
        public IActionResult CreateOrder([FromBody] OrderRequest request)
        {
            try
            {

                if (request == null || request.Items == null || !request.Items.Any())
                {
                    return BadRequest(new { success = false, message = "Order data cannot be empty or your cart contains no items." });
                }

                decimal totalAmount = 0;

                foreach (var item in request.Items)
                {
                    var jersey = ProductList.FirstOrDefault(p => p.Id == item.ProductId);

                    if (jersey == null)
                    {
                        return NotFound(new { success = false, message = $"Order failed: product with ID {item.ProductId} was not found in our system." });
                    }

                    if (jersey.Stock < item.Quantity)
                    {
                        return BadRequest(new { success = false, message = $"Order failed: not enough stock for {jersey.TeamName}. Remaining stock: {jersey.Stock}" });
                    }

                    totalAmount += jersey.Price * item.Quantity;
                }

                foreach (var item in request.Items)
                {
                    var jersey = ProductList.First(p => p.Id == item.ProductId);
                    jersey.Stock -= item.Quantity;
                }

                Random rnd = new Random();
                string orderNumber = "HGN" + rnd.Next(1000000, 9999999).ToString();

                var response = new
                {
                    success = true,
                    message = "Order created successfully.",
                    orderDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    totalAmount = totalAmount,
                    orderNumber = orderNumber
                };

                return Ok(response);
            }
            catch (Exception ex)
            {

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
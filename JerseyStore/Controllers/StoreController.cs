using JerseyStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace JerseyStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
   
        private static readonly List<Product> ProductList = new List<Product>
        {
            new Product { Id = 1, TeamName = "Fenerbahçe", Price = 5999, Stock = 120 },
            new Product { Id = 2, TeamName = "Galatasaray", Price = 5900, Stock = 150 },
            new Product { Id = 3, TeamName = "Beşiktaş", Price = 5800, Stock = 130 }
        };


        [HttpGet("products")]
        public IActionResult GetProducts()
        {
            return Ok(ProductList);
        }


        [HttpPost("order")]
        public IActionResult CreateOrder([FromBody] OrderRequest request)
        {
            decimal totalAmount = 0;


            foreach (var item in request.Items)
            {

                var product = ProductList.FirstOrDefault(p => p.Id == item.ProductId);

   
                if (product != null)
                {
                    totalAmount += product.Price * item.Quantity;
                }
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
    }
}
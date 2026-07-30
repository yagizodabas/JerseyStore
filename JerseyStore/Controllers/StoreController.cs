using JerseyStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace JerseyStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        // JerseyStore konseptine uygun hafızadaki geçici ürün listemiz
        private static readonly List<Product> ProductList = new List<Product>
        {
            new Product { Id = 1, TeamName = "Fenerbahçe", Price = 5999, Stock = 120 },
            new Product { Id = 2, TeamName = "Galatasaray", Price = 5900, Stock = 150 },
            new Product { Id = 3, TeamName = "Beşiktaş", Price = 5800, Stock = 130 }
        };

        // 1. ÖDEV: HttpGet ile parametresiz ürün listeleme
        [HttpGet("products")]
        public IActionResult GetProducts()
        {
            return Ok(ProductList);
        }

        // 2. ÖDEV: HttpPost ile sipariş oluşturma
        [HttpPost("order")]
        public IActionResult CreateOrder([FromBody] OrderRequest request)
        {
            decimal totalAmount = 0;

            // 1. Gelen siparişteki ürünlerin toplam fiyatını hesaplıyoruz
            foreach (var item in request.Items)
            {
                // Müşterinin gönderdiği ProductId ile sistemdeki formaları eşleştiriyoruz
                var product = ProductList.FirstOrDefault(p => p.Id == item.ProductId);

                // Ürün sistemde varsa fiyata ekle
                if (product != null)
                {
                    totalAmount += product.Price * item.Quantity;
                }
            }

            // 2. Benzersiz Sipariş Numarası Üretme (Örn: HGN + Rastgele 7 haneli sayı)
            Random rnd = new Random();
            string orderNumber = "HGN" + rnd.Next(1000000, 9999999).ToString();

            // 3. Danışmanının tam olarak beklediği JSON yanıt formatını oluşturma
            var response = new
            {
                success = true,
                message = "Sipariş başarıyla oluşturuldu.",
                orderDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                totalAmount = totalAmount,
                orderNumber = orderNumber
            };

            // 4. Yanıtı geri dönüyoruz
            return Ok(response);
        }
    }
}
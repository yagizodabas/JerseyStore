using Microsoft.AspNetCore.Mvc;

namespace JerseyStore.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
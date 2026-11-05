using Microsoft.AspNetCore.Mvc;

namespace JerseyStore.Controllers
{
    public class OrdersController : Controller
    {
        public IActionResult Cart()
        {
            return View();
        }
    }
}
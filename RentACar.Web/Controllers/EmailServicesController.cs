using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class EmailServicesController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction(nameof(EmailServicesHub));
        }

        [HttpGet]
        public IActionResult EmailServicesHub()
        {
            return View();
        }
    }
}

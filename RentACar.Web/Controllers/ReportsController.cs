using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RentACar.Web.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class ReportsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
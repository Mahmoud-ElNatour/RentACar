using Microsoft.AspNetCore.Mvc;

namespace RentACar.Web.Controllers
{
    public class ControlPanelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

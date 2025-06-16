using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RentACar.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Employee")]
    public class ReportsController : Controller
    {
        [HttpGet("~/Reports")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return View();
        }
    }
}

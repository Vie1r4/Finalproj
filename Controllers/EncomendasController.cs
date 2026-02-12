using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finalproj.Controllers
{
    [Authorize]
    public class EncomendasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

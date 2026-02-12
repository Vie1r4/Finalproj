using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finalproj.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

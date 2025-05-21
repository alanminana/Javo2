using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace Javo2.Controllers
{
    [Authorize]
    public class SpaController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.UseVue = true;
            ViewBag.Title = "The BuryCode";

            // Obtener permisos del usuario para pasar a Vue
            var permisos = User.Claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToArray();
            ViewBag.UserPermissions = JsonSerializer.Serialize(permisos);

            return View();
        }
    }
}
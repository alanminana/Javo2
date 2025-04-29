using Javo2.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Javo2.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public HomeController(ILogger<HomeController> logger)
            : base(logger)
        {
        }
        // En HomeController.cs
        public IActionResult Diagnostico()
        {
            var diag = new Dictionary<string, string>
            {
                ["Directorio Raíz"] = Directory.GetCurrentDirectory(),
                ["Ruta Usuarios"] = Path.Combine(Directory.GetCurrentDirectory(), "Data/usuarios.json"),
                ["Usuarios Existe"] = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Data/usuarios.json")).ToString(),
                ["Ruta Roles"] = Path.Combine(Directory.GetCurrentDirectory(), "Data/roles.json"),
                ["Roles Existe"] = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Data/roles.json")).ToString(),
                ["Ruta Permisos"] = Path.Combine(Directory.GetCurrentDirectory(), "Data/permisos.json"),
                ["Permisos Existe"] = System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Data/permisos.json")).ToString(),
                ["Usuario Autenticado"] = User.Identity.IsAuthenticated.ToString(),
                ["Usuario"] = User.Identity.Name ?? "Ninguno"
            };

            return View(diag);
        }
        public IActionResult Index()
        {
            _logger.LogInformation("Home Index called");
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy page called");
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("Error action called");
            return View();
        }
    }
}
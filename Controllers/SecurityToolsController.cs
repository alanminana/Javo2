// Controllers/SecurityToolsController.cs
using Javo2.Controllers.Base;
using Javo2.Data.Seeders;
using Javo2.IServices.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SecurityToolsController : BaseController
    {
        private readonly IPermisoService _permisoService;
        private readonly IRolService _rolService;

        public SecurityToolsController(
            IPermisoService permisoService,
            IRolService rolService,
            ILogger<SecurityToolsController> logger)
            : base(logger)
        {
            _permisoService = permisoService;
            _rolService = rolService;
        }

        // GET: SecurityTools/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: SecurityTools/RecargarPermisos
        public async Task<IActionResult> RecargarPermisos()
        {
            try
            {
                // Obtener las dependencias del contenedor
                var permisoService = HttpContext.RequestServices.GetRequiredService<IPermisoService>();
                var rolService = HttpContext.RequestServices.GetRequiredService<IRolService>();
                var loggerFactory = HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var permissionLogger = loggerFactory.CreateLogger<PermissionSeeder>();

                var seeder = new PermissionSeeder(permisoService, rolService, permissionLogger);
                await seeder.SeedAsync();

                TempData["Success"] = "Permisos recargados correctamente";
                return RedirectToAction("Index", "SecurityDashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recargar permisos");
                TempData["Error"] = "Error al recargar permisos: " + ex.Message;
                return RedirectToAction("Index", "SecurityDashboard");
            }
        }

        // GET: SecurityTools/CerrarSesiones
        public IActionResult CerrarSesiones()
        {
            // Implementación futura para cerrar sesiones activas
            TempData["Info"] = "Función no implementada todavía";
            return RedirectToAction("Index", "SecurityDashboard");
        }
    }
}
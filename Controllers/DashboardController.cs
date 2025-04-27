// 1. Controllers/Security/DashboardController.cs (continuación)
using Javo2.IServices.Authentication;
using Javo2.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Security
{
    [Authorize]
    public class SecurityDashboardController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IPermisoService _permisoService;
        private readonly ILogger<SecurityDashboardController> _logger;

        public SecurityDashboardController(
            IUsuarioService usuarioService,
            IRolService rolService,
            IPermisoService permisoService,
            ILogger<SecurityDashboardController> logger)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
            _permisoService = permisoService;
            _logger = logger;
        }

        [Authorize(Policy = "Permission:configuracion.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener estadísticas para el dashboard
                var usuarios = await _usuarioService.GetAllUsuariosAsync();
                var roles = await _rolService.GetAllRolesAsync();
                var permisos = await _permisoService.GetAllPermisosAsync();

                var dashboardViewModel = new SecurityDashboardViewModel
                {
                    TotalUsuarios = usuarios.Count(),
                    UsuariosActivos = usuarios.Count(u => u.Activo),
                    TotalRoles = roles.Count(),
                    TotalPermisos = permisos.Count(),
                    UltimosUsuariosRegistrados = usuarios
                        .OrderByDescending(u => u.FechaCreacion)
                        .Take(5)
                        .Select(u => new UsuarioSimpleViewModel
                        {
                            UsuarioID = u.UsuarioID,
                            NombreUsuario = u.NombreUsuario,
                            NombreCompleto = $"{u.Nombre} {u.Apellido}",
                            FechaCreacion = u.FechaCreacion
                        })
                        .ToList(),
                    UltimosAccesos = usuarios
                        .Where(u => u.UltimoAcceso.HasValue)
                        .OrderByDescending(u => u.UltimoAcceso)
                        .Take(5)
                        .Select(u => new UsuarioSimpleViewModel
                        {
                            UsuarioID = u.UsuarioID,
                            NombreUsuario = u.NombreUsuario,
                            NombreCompleto = $"{u.Nombre} {u.Apellido}",
                            UltimoAcceso = u.UltimoAcceso
                        })
                        .ToList()
                };

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el dashboard de seguridad");
                return View("Error");
            }
        }
    }
}


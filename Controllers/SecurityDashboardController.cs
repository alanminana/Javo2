// Controllers/SecurityDashboardController.cs
using Javo2.Controllers.Base;
using Javo2.Helpers;
using Javo2.IServices.Authentication;
using Javo2.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize]
    public class SecurityDashboardController : BaseController
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IPermisoService _permisoService;

        public SecurityDashboardController(
            IUsuarioService usuarioService,
            IRolService rolService,
            IPermisoService permisoService,
            ILogger<SecurityDashboardController> logger)
            : base(logger)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
            _permisoService = permisoService;
        }

        // GET: SecurityDashboard/Index
        [Authorize(Policy = "Permission:securitydashboard.ver")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener estadísticas para el dashboard
                var usuarios = await _usuarioService.GetAllUsuariosAsync();
                var roles = await _rolService.GetAllRolesAsync();
                var permisos = await _permisoService.GetAllPermisosAsync();

                var viewModel = new SecurityDashboardViewModel
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

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el dashboard de seguridad");
                return View("Error");
            }
        }

        // GET: SecurityDashboard/Usuarios
        [Authorize(Policy = "Permission:usuarios.ver")]
        public async Task<IActionResult> Usuarios()
        {
            try
            {
                var usuarios = await _usuarioService.GetAllUsuariosAsync();
                return View(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de usuarios");
                return View("Error");
            }
        }

        // GET: SecurityDashboard/Roles
        [Authorize(Policy = "Permission:roles.ver")]
        public async Task<IActionResult> Roles()
        {
            try
            {
                var roles = await _rolService.GetAllRolesAsync();
                return View(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de roles");
                return View("Error");
            }
        }

        // GET: SecurityDashboard/Permisos
        [Authorize(Policy = "Permission:permisos.ver")]
        public async Task<IActionResult> Permisos()
        {
            try
            {
                var permisos = await _permisoService.GetAllPermisosAsync();
                var permisosAgrupados = permisos.GroupBy(p => p.Grupo ?? "General")
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Nombre).ToList());

                return View(permisosAgrupados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de permisos");
                return View("Error");
            }
        }
    }
}
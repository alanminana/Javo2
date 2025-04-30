// Controllers/DiagnosticController.cs
using Javo2.Controllers.Base;
using Javo2.IServices.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class DiagnosticController : BaseController
    {
        public DiagnosticController(ILogger<DiagnosticController> logger) : base(logger)
        {
        }

        public IActionResult Index()
        {
            var diagnosticInfo = new Dictionary<string, string>
            {
                { "Sistema Operativo", Environment.OSVersion.ToString() },
                { "Versión .NET", Environment.Version.ToString() },
                { "Fecha del Servidor", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "Directorio de la aplicación", Environment.CurrentDirectory },
                { "Memoria RAM Disponible", (Environment.WorkingSet / 1024 / 1024).ToString() + " MB" },
                { "Procesadores", Environment.ProcessorCount.ToString() },
                { "Usuario del sistema", Environment.UserName }
            };

            return View(diagnosticInfo);
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> VerificarPermisos()
        {
            try
            {
                var usuarioService = HttpContext.RequestServices.GetRequiredService<IUsuarioService>();
                var rolService = HttpContext.RequestServices.GetRequiredService<IRolService>();
                var permisoService = HttpContext.RequestServices.GetRequiredService<IPermisoService>();

                var username = User.Identity.Name;
                var usuario = await usuarioService.GetUsuarioByNombreUsuarioAsync(username);
                if (usuario == null)
                {
                    return new JsonResult(new { success = false, message = "Usuario no encontrado" });
                }

                var roles = new List<string>();
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await rolService.GetRolByIDAsync(usuarioRol.RolID);
                    if (rol != null)
                    {
                        roles.Add(rol.Nombre);
                    }
                }

                var permisos = await usuarioService.GetPermisosUsuarioAsync(usuario.UsuarioID);
                var permisosLista = permisos.Select(p => p.Codigo).ToList();

                var tienePermisoDashboard = permisosLista.Contains("securitydashboard.ver");

                return new JsonResult(new
                {
                    success = true,
                    usuario = new
                    {
                        username = usuario.NombreUsuario,
                        nombre = $"{usuario.Nombre} {usuario.Apellido}",
                        roles = roles,
                        tieneRolAdmin = roles.Any(r => r.Equals("Administrador", StringComparison.OrdinalIgnoreCase)),
                        permisos = permisosLista,
                        tienePermisoDashboard = tienePermisoDashboard
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
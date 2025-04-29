// DiagnosticController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Javo2.IServices.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Javo2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DiagnosticController : ControllerBase
    {
        private readonly ILogger<DiagnosticController> _logger;
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IPermisoService _permisoService;

        public DiagnosticController(
            ILogger<DiagnosticController> logger,
            IUsuarioService usuarioService,
            IRolService rolService,
            IPermisoService permisoService)
        {
            _logger = logger;
            _usuarioService = usuarioService;
            _rolService = rolService;
            _permisoService = permisoService;
        }

        [HttpGet("files")]
        public IActionResult CheckFiles()
        {
            var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            var files = new string[] { "usuarios.json", "roles.json", "permisos.json", "configuracion.json", "passwordResetTokens.json" };

            var result = new Dictionary<string, object>();

            result["directory_exists"] = Directory.Exists(dataDir);
            result["root_path"] = Directory.GetCurrentDirectory();
            result["data_path"] = dataDir;

            var fileResults = new Dictionary<string, object>();
            foreach (var file in files)
            {
                var path = Path.Combine(dataDir, file);
                try
                {
                    fileResults[file] = new
                    {
                        exists = System.IO.File.Exists(path),
                        size = System.IO.File.Exists(path) ? new FileInfo(path).Length : 0,
                        content = System.IO.File.Exists(path) && new FileInfo(path).Length < 1024 * 10
                            ? System.IO.File.ReadAllText(path)
                            : "File too large to display"
                    };
                }
                catch (Exception ex)
                {
                    fileResults[file] = new { error = ex.Message };
                }
            }
            result["files"] = fileResults;

            return Ok(result);
        }

        [HttpGet("auth")]
        public async Task<IActionResult> CheckAuthServices()
        {
            var result = new Dictionary<string, object>();

            try
            {
                var usuarios = await _usuarioService.GetAllUsuariosAsync();
                result["usuarios_count"] = usuarios.Count();
                result["usuarios"] = usuarios.Select(u => new {
                    u.UsuarioID,
                    u.NombreUsuario,
                    u.Email,
                    roles_count = u.Roles?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                result["usuarios_error"] = ex.Message;
            }

            try
            {
                var roles = await _rolService.GetAllRolesAsync();
                result["roles_count"] = roles.Count();
                result["roles"] = roles.Select(r => new {
                    r.RolID,
                    r.Nombre,
                    permisos_count = r.Permisos?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                result["roles_error"] = ex.Message;
            }

            try
            {
                var permisos = await _permisoService.GetAllPermisosAsync();
                result["permisos_count"] = permisos.Count();
                result["permisos"] = permisos.Select(p => new {
                    p.PermisoID,
                    p.Nombre,
                    p.Codigo,
                    p.Grupo,
                    p.Activo
                });
            }
            catch (Exception ex)
            {
                result["permisos_error"] = ex.Message;
            }

            return Ok(result);
        }

        [HttpGet("user")]
        public IActionResult CheckCurrentUser()
        {
            var result = new Dictionary<string, object>();

            result["is_authenticated"] = User.Identity.IsAuthenticated;
            result["identity_name"] = User.Identity.Name;

            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            result["claims"] = claims;

            var permissions = User.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();

            result["permissions"] = permissions;

            var roles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            result["roles"] = roles;

            return Ok(result);
        }
    }
}
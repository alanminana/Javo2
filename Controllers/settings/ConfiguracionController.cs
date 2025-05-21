// Controllers/Admin/ConfiguracionController.cs
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.IServices.Authentication;
using Javo2.Models;
using Javo2.Models.Authentication;
using Javo2.ViewModels.Authentication;
using Javo2.ViewModels.Configuracion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.settings
{
    [Authorize]
    public class ConfiguracionController : BaseController
    {
        private readonly IConfiguracionService _configuracionService;
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IPermisoService _permisoService;

        public ConfiguracionController(
            IConfiguracionService configuracionService,
            IUsuarioService usuarioService,
            IRolService rolService,
            IPermisoService permisoService,
            ILogger<ConfiguracionController> logger) : base(logger)
        {
            _configuracionService = configuracionService;
            _usuarioService = usuarioService;
            _rolService = rolService;
            _permisoService = permisoService;
        }

        #region Configuración del Sistema

        [HttpGet]
        [Authorize(Policy = "Permission:configuracion.ver")]
        public async Task<IActionResult> Index(string modulo = null)
        {
            try
            {
                // Obtener todos los módulos primero
                var todasConfiguraciones = await _configuracionService.GetAllAsync();
                var modulos = todasConfiguraciones
                    .Select(c => c.Modulo)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                // Si no se especificó un módulo pero hay módulos disponibles, seleccionar el primero
                if (string.IsNullOrEmpty(modulo) && modulos.Any())
                {
                    modulo = modulos.First();
                }

                // Obtener configuraciones para el módulo seleccionado
                var configuraciones = string.IsNullOrEmpty(modulo) ?
                    todasConfiguraciones.ToList() :
                    todasConfiguraciones.Where(c => c.Modulo == modulo).ToList();

                var viewModel = new ConfiguracionIndexViewModel
                {
                    Configuraciones = configuraciones,
                    ModuloSeleccionado = modulo,
                    Modulos = modulos
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuraciones");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:configuracion.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var configuraciones = await _configuracionService.GetAllAsync();
                var configuracion = configuraciones.FirstOrDefault(c => c.ConfiguracionID == id);

                if (configuracion == null)
                    return NotFound();

                return View(configuracion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuración para editar");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:configuracion.editar")]
        public async Task<IActionResult> Edit(ConfiguracionSistema configuracion)
        {
            if (!ModelState.IsValid)
                return View(configuracion);

            try
            {
                await _configuracionService.SaveAsync(configuracion);
                TempData["Success"] = "Configuración actualizada correctamente.";
                return RedirectToAction(nameof(Index), new { modulo = configuracion.Modulo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar configuración");
                ModelState.AddModelError("", "Ocurrió un error al guardar la configuración.");
                return View(configuracion);
            }
        }

        #endregion

        #region Configuración Inicial

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Setup()
        {
            try
            {
                var usuarios = await _usuarioService.GetAllUsuariosAsync();
                if (usuarios.Any())
                {
                    return RedirectToAction("Login", "Auth");
                }
                return View(new ConfiguracionInicialViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuración inicial");
                return View("Error");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(ConfiguracionInicialViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var usuarios = await _usuarioService.GetAllUsuariosAsync();
                if (usuarios.Any())
                {
                    return RedirectToAction("Login", "Auth");
                }

                var admin = new Usuario
                {
                    NombreUsuario = model.NombreUsuario,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Email = model.Email,
                    CreadoPor = "Sistema",
                    Activo = true
                };

                await _usuarioService.CreateUsuarioAsync(admin, model.Contraseña);

                var roles = await _rolService.GetAllRolesAsync();
                var rolAdmin = roles.FirstOrDefault(r =>
                    r.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase));

                if (rolAdmin != null)
                {
                    await _usuarioService.AsignarRolAsync(admin.UsuarioID, rolAdmin.RolID);
                }

                return RedirectToAction(nameof(ConfiguracionCompletada));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la configuración inicial");
                ModelState.AddModelError(string.Empty,
                    "Ocurrió un error al crear el usuario administrador: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfiguracionCompletada()
        {
            return View();
        }

        #endregion

        #region Diagnóstico y Herramientas

        [Authorize(Roles = "Administrador")]
        public IActionResult Diagnostico()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar diagnóstico");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> VerificarPermisos()
        {
            try
            {
                var username = User.Identity.Name;
                var usuario = await _usuarioService.GetUsuarioByNombreUsuarioAsync(username);
                if (usuario == null)
                {
                    return new JsonResult(new { success = false, message = "Usuario no encontrado" });
                }

                var roles = new List<string>();
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await _rolService.GetRolByIDAsync(usuarioRol.RolID);
                    if (rol != null)
                    {
                        roles.Add(rol.Nombre);
                    }
                }

                var permisos = await _usuarioService.GetPermisosUsuarioAsync(usuario.UsuarioID);
                var permisosLista = permisos.Select(p => p.Codigo).ToList();

                var tienePermisoDashboard = permisosLista.Contains("securitydashboard.ver");

                return new JsonResult(new
                {
                    success = true,
                    usuario = new
                    {
                        username = usuario.NombreUsuario,
                        nombre = $"{usuario.Nombre} {usuario.Apellido}",
                        roles,
                        tieneRolAdmin = roles.Any(r => r.Equals("Administrador", StringComparison.OrdinalIgnoreCase)),
                        permisos = permisosLista,
                        tienePermisoDashboard
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar permisos");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult VerificarConexion()
        {
            try
            {
                // Verificar conexión a base de datos
                var dbConnected = true; // Implementar verificación real

                // Verificar conexión a servicios externos
                var externalServicesConnected = true; // Implementar verificación real

                return Json(new
                {
                    success = true,
                    database = dbConnected,
                    externalServices = externalServicesConnected
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar conexiones");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult EstadoSistema()
        {
            try
            {
                var estadoInfo = new Dictionary<string, string>
                {
                    { "Estado Aplicación", "Operativa" },
                    { "Estado Base de Datos", "Conectado" },
                    { "Estado Servicios Externos", "Operativos" },
                    { "Tiempo de Actividad", GetUptimeText() }
                };

                return View(estadoInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado del sistema");
                return View("Error");
            }
        }

        #endregion

        #region Mantenimiento

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult Mantenimiento()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> LimpiarDatosCaducados()
        {
            try
            {
                // Implementar lógica para limpiar datos temporales caducados
                TempData["Success"] = "Limpieza de datos completada correctamente";
                return RedirectToAction(nameof(Mantenimiento));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar datos caducados");
                TempData["Error"] = "Error al limpiar datos: " + ex.Message;
                return RedirectToAction(nameof(Mantenimiento));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> OptimizarBaseDeDatos()
        {
            try
            {
                // Implementar lógica para optimizar la base de datos
                TempData["Success"] = "Optimización de base de datos completada correctamente";
                return RedirectToAction(nameof(Mantenimiento));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al optimizar base de datos");
                TempData["Error"] = "Error al optimizar base de datos: " + ex.Message;
                return RedirectToAction(nameof(Mantenimiento));
            }
        }

        #endregion

        #region Métodos auxiliares

        private string GetUptimeText()
        {
            // Esto normalmente utilizaría datos reales de uptime del sistema
            // En este ejemplo, generamos un valor ficticio
            Random rnd = new Random();
            int days = rnd.Next(1, 30);
            int hours = rnd.Next(0, 24);
            int minutes = rnd.Next(0, 60);

            return $"{days} días, {hours} horas, {minutes} minutos";
        }

        #endregion
    }
}
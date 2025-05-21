// Controllers/Clientes/ClientesController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.Filters;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Clientes
{
    [Authorize(Policy = "PermisoPolitica")]
    [TypeFilter(typeof(ClientesExceptionFilter))]
    public class ClientesController : BaseController
    {
        private readonly IClienteService _clienteService;
        private readonly IGaranteService _garanteService;
        private readonly ICreditoService _creditoService;
        private readonly IMapper _mapper;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(
            IClienteService clienteService,
            IGaranteService garanteService,
            ICreditoService creditoService,
            IMapper mapper,
            ILogger<ClientesController> logger)
            : base(logger)
        {
            _clienteService = clienteService;
            _garanteService = garanteService;
            _creditoService = creditoService;
            _mapper = mapper;
            _logger = logger;
        }

        #region Gestión de Clientes (CRUD)

        [HttpGet]
        [Authorize(Policy = "Permission:clientes.ver")]
        public async Task<IActionResult> Index(string searchTerm = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var (clientes, totalCount) = await _clienteService.SearchClientesAsync(searchTerm, page, pageSize);
                var model = _mapper.Map<IEnumerable<ClientesViewModel>>(clientes);

                ViewBag.SearchTerm = searchTerm;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                ViewBag.TotalCount = totalCount;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Index");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:clientes.crear")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new ClientesViewModel
                {
                    Provincias = await ObtenerProvincias(),
                    Ciudades = new List<SelectListItem>()
                };
                return View("Form", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Create GET");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:clientes.crear")]
        public async Task<IActionResult> Create(ClientesViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    model.Provincias = await ObtenerProvincias();
                    model.Ciudades = await ObtenerCiudades(model.ProvinciaID);
                    return View("Form", model);
                }

                var cliente = _mapper.Map<Cliente>(model);
                await _clienteService.CreateClienteAsync(cliente);

                TempData["Success"] = "Cliente creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Create POST");
                ModelState.AddModelError("", "Ocurrió un error al crear el cliente");
                model.Provincias = await ObtenerProvincias();
                model.Ciudades = await ObtenerCiudades(model.ProvinciaID);
                return View("Form", model);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:clientes.editar")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var cliente = await _clienteService.GetClienteByIDAsync(id);
                if (cliente == null)
                    return NotFound();

                var viewModel = _mapper.Map<ClientesViewModel>(cliente);
                viewModel.Provincias = await ObtenerProvincias();
                viewModel.Ciudades = await ObtenerCiudades(viewModel.ProvinciaID);

                return View("Form", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Edit GET");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:clientes.editar")]
        public async Task<IActionResult> Edit(int id, ClientesViewModel model)
        {
            try
            {
                // Añadir logs para verificar los valores recibidos
                _logger.LogInformation("Edit recibido - ScoreCredito: {0}, VencimientoCuotas: {1}, IngresosMensuales: {2}",
                    model.ScoreCredito, model.VencimientoCuotas, model.IngresosMensuales);

                if (id != model.ClienteID)
                    return BadRequest();

                if (!ModelState.IsValid)
                {
                    model.Provincias = await ObtenerProvincias();
                    model.Ciudades = await ObtenerCiudades(model.ProvinciaID);
                    return View("Form", model);
                }

                // Asegurarse de que los campos de crédito se mapean correctamente
                var cliente = _mapper.Map<Cliente>(model);

                // Verificar que los valores están en el objeto cliente
                _logger.LogInformation("Cliente mapeado - ScoreCredito: {0}, VencimientoCuotas: {1}, IngresosMensuales: {2}",
                    cliente.ScoreCredito, cliente.VencimientoCuotas, cliente.IngresosMensuales);

                await _clienteService.UpdateClienteAsync(cliente);
                TempData["Success"] = "Cliente actualizado exitosamente";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Edit POST");
                ModelState.AddModelError("", "Ocurrió un error al actualizar el cliente");
                model.Provincias = await ObtenerProvincias();
                model.Ciudades = await ObtenerCiudades(model.ProvinciaID);
                return View("Form", model);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:clientes.ver")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var cliente = await _clienteService.GetClienteByIDAsync(id);
                if (cliente == null)
                    return NotFound();

                var viewModel = _mapper.Map<ClientesViewModel>(cliente);

                // Cargar datos del garante si existe
                if (cliente.GaranteID.HasValue)
                {
                    var garante = await _garanteService.GetGaranteByIdAsync(cliente.GaranteID.Value);
                    if (garante != null)
                    {
                        viewModel.NombreGarante = $"{garante.Nombre} {garante.Apellido}";
                        viewModel.Garante = _mapper.Map<GaranteViewModel>(garante);
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles del cliente");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:clientes.eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cliente = await _clienteService.GetClienteByIDAsync(id);
                if (cliente == null)
                    return NotFound();

                var viewModel = _mapper.Map<ClientesViewModel>(cliente);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Delete GET");
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:clientes.eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _clienteService.DeleteClienteAsync(id);
                TempData["Success"] = "Cliente eliminado exitosamente";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DeleteConfirmed");
                TempData["Error"] = "Ocurrió un error al eliminar el cliente";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Gestión de Garantes

        [HttpGet]
        [Authorize(Policy = "Permission:clientes.editar")]
        public async Task<IActionResult> AsignarGarante(int id)
        {
            try
            {
                var cliente = await _clienteService.GetClienteByIDAsync(id);
                if (cliente == null) return NotFound();

                var viewModel = new GaranteViewModel
                {
                    ClienteID = id,
                    NombreCliente = $"{cliente.Nombre} {cliente.Apellido}",
                    Provincias = await ObtenerProvincias(),
                    Ciudades = new List<SelectListItem>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de garante");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:clientes.editar")]
        public async Task<IActionResult> AsignarGarante(GaranteViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Provincias = await ObtenerProvincias();
                    model.Ciudades = await ObtenerCiudades(model.ProvinciaID);
                    return View(model);
                }

                // Mapear solo al modelo Garante, evitando propiedades innecesarias
                var garante = new Garante
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    DNI = model.DNI,
                    Email = model.Email,
                    Telefono = model.Telefono,
                    Celular = model.Celular,
                    Calle = model.Calle,
                    NumeroCalle = model.NumeroCalle,
                    NumeroPiso = model.NumeroPiso,
                    Dpto = model.Dpto,
                    Localidad = model.Localidad,
                    CodigoPostal = model.CodigoPostal,
                    ProvinciaID = model.ProvinciaID,
                    CiudadID = model.CiudadID,
                    LugarTrabajo = model.LugarTrabajo,
                    IngresosMensuales = model.IngresosMensuales,
                    RelacionCliente = model.RelacionCliente,
                    FechaCreacion = DateTime.UtcNow
                };

                var createdGarante = await _garanteService.CreateGaranteAsync(garante);

                // Asignar garante al cliente
                await _clienteService.AsignarGaranteAsync(model.ClienteID, createdGarante.GaranteID);

                TempData["Success"] = "Garante asignado correctamente";
                return RedirectToAction(nameof(Details), new { id = model.ClienteID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar garante");
                ModelState.AddModelError("", "Error al asignar garante: " + ex.Message);
                model.Provincias = await ObtenerProvincias();
                model.Ciudades = await ObtenerCiudades(model.ProvinciaID);
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:clientes.ver")]
        [Route("Clientes/VerGarante/{id}")]
        public async Task<IActionResult> VerGarante(int id, int? clienteId = null)
        {
            _logger.LogInformation("VerGarante llamado con id={Id}, clienteId={ClienteId}", id, clienteId);

            try
            {
                var garante = await _garanteService.GetGaranteByIdAsync(id);
                if (garante == null)
                {
                    _logger.LogWarning("Garante con ID {Id} no encontrado", id);
                    return NotFound();
                }

                ViewBag.ClienteID = clienteId;
                return View(garante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al mostrar detalles del garante");
                return View("Error");
            }
        }

        #endregion

        #region Gestión de Créditos

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:clientes.editar")]
        public async Task<IActionResult> AjustarCredito(int id, decimal nuevoLimite)
        {
            try
            {
                await _clienteService.AjustarLimiteCreditoAsync(id, nuevoLimite, User.Identity?.Name ?? "Sistema");
                TempData["Success"] = $"Límite de crédito ajustado a {nuevoLimite:C}";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ajustar límite de crédito para cliente ID: {ID}", id);
                TempData["Error"] = "Error al ajustar límite de crédito: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Clientes/ConfiguracionCredito
        [HttpGet]
        [Authorize(Policy = "Permission:configuracion.editar")]
        public async Task<IActionResult> ConfiguracionCredito()
        {
            try
            {
                var config = await _creditoService.GetConfiguracionVigenteAsync();
                return View(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuración de crédito");
                return View("Error");
            }
        }

        // POST: Clientes/ConfiguracionCredito
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:configuracion.editar")]
        public async Task<IActionResult> ConfiguracionCredito(ConfiguracionCredito model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                model.ModificadoPor = User.Identity?.Name ?? "Sistema";
                model.FechaModificacion = DateTime.Now;

                await _creditoService.SaveConfiguracionAsync(model);
                TempData["Success"] = "Configuración de crédito actualizada correctamente";

                return RedirectToAction(nameof(ConfiguracionCredito));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar configuración de crédito");
                ModelState.AddModelError("", "Error al guardar configuración: " + ex.Message);
                return View(model);
            }
        }

        // GET: Clientes/CriteriosCredito
        [HttpGet]
        [Authorize(Policy = "Permission:configuracion.editar")]
        public async Task<IActionResult> CriteriosCredito()
        {
            try
            {
                var criterios = await _creditoService.GetAllCriteriosAsync();
                return View(criterios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar criterios de crédito");
                return View("Error");
            }
        }

        // GET: Clientes/EditarCriterio/A
        [HttpGet]
        [Authorize(Policy = "Permission:configuracion.editar")]
        public async Task<IActionResult> EditarCriterio(string id)
        {
            try
            {
                var criterio = await _creditoService.GetCriterioByScoreAsync(id);
                if (criterio == null)
                {
                    criterio = new CriteriosCalificacionCredito { ScoreCredito = id };
                }

                return View(criterio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar criterio de crédito");
                return View("Error");
            }
        }

        // POST: Clientes/EditarCriterio
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:configuracion.editar")]
        public async Task<IActionResult> EditarCriterio(CriteriosCalificacionCredito model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                model.ModificadoPor = User.Identity?.Name ?? "Sistema";
                model.FechaModificacion = DateTime.Now;

                await _creditoService.SaveCriterioAsync(model);
                TempData["Success"] = $"Criterio para calificación {model.ScoreCredito} actualizado correctamente";

                return RedirectToAction(nameof(CriteriosCredito));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar criterio de crédito");
                ModelState.AddModelError("", "Error al guardar criterio: " + ex.Message);
                return View(model);
            }
        }

        #endregion

        #region Métodos de Filtrado y Búsqueda

        [HttpPost]
        public async Task<IActionResult> Filter(string filterField, string filterValue)
        {
            try
            {
                var clientes = await _clienteService.GetAllClientesAsync();
                var viewModels = _mapper.Map<IEnumerable<ClientesViewModel>>(clientes);

                if (!string.IsNullOrEmpty(filterValue))
                {
                    viewModels = filterField switch
                    {
                        "nombre" => viewModels.Where(c => c.Nombre.Contains(filterValue, StringComparison.OrdinalIgnoreCase) ||
                                                        c.Apellido.Contains(filterValue, StringComparison.OrdinalIgnoreCase)),
                        "dni" => viewModels.Where(c => c.DNI.ToString().Contains(filterValue)),
                        "localidad" => viewModels.Where(c => c.Localidad.Contains(filterValue, StringComparison.OrdinalIgnoreCase)),
                        "email" => viewModels.Where(c => c.Email.Contains(filterValue, StringComparison.OrdinalIgnoreCase)),
                        _ => viewModels,
                    };
                }

                return PartialView("_ClientesTable", viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar clientes");
                return PartialView("_ClientesTable", new List<ClientesViewModel>());
            }
        }

        #endregion

        #region Métodos Auxiliares para Ubicación

        [HttpGet]
        public async Task<IActionResult> GetCiudades(int provinciaID)
        {
            try
            {
                var ciudades = await _clienteService.GetCiudadesByProvinciaAsync(provinciaID);
                var selectList = ciudades.Select(c => new SelectListItem
                {
                    Value = c.CiudadID.ToString(),
                    Text = c.Nombre
                }).ToList();

                return Json(selectList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetCiudades");
                return Json(new List<SelectListItem>());
            }
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerProvincias()
        {
            var provincias = await _clienteService.GetProvinciasAsync();
            return provincias.Select(p => new SelectListItem
            {
                Value = p.ProvinciaID.ToString(),
                Text = p.Nombre
            });
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCiudades(int provinciaID)
        {
            var ciudades = await _clienteService.GetCiudadesByProvinciaAsync(provinciaID);
            return ciudades.Select(c => new SelectListItem
            {
                Value = c.CiudadID.ToString(),
                Text = c.Nombre
            });
        }

        #endregion
    }
}
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.Filters;
using Javo2.IServices;
using Javo2.Models;
using Javo2.Services;
using Javo2.ViewModels.Operaciones.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize(Policy = "PermisoPolitica")]
    [TypeFilter(typeof(ClientesExceptionFilter))]

    public class ClientesController : BaseController
    {
        private readonly IClienteService _clienteService;
        private readonly IClienteSearchService _searchService;
        private readonly IGaranteService _garanteService; // Añade este campo
        private readonly IMapper _mapper;
        private readonly ILogger<ClientesController> _logger;


        public ClientesController(
     IClienteService clienteService,
     IMapper mapper,
     IGaranteService garanteService, // Inyectamos el servicio
     ILogger<ClientesController> logger)
     : base(logger)
        {
            _clienteService = clienteService;
            _searchService = clienteService as IClienteSearchService;
            _garanteService = garanteService; // Asignamos el servicio
            _mapper = mapper;
            _logger = logger;
        }

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

        // GET: Clientes/AsignarGarante/5
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

        // POST: Clientes/AsignarGarante
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

                // Mapear solo al modelo Garante, evitando propiedades como
                // 
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

        public async Task<IActionResult> Index(string searchTerm = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var (clientes, totalCount) = await _searchService.SearchClientesAsync(searchTerm, page, pageSize);
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

        public async Task<IActionResult> Create()
        {
            try
            {
                var viewModel = new ClientesViewModel
                {
                    Provincias = await ObtenerProvincias(),
                    Ciudades = new List<SelectListItem>()
                };
                return View("Form", viewModel); // Asegúrate que sea "Form" y no "AsignarGarante"
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Create GET");
                return View("Error");
            }
        }
        // Controllers/ClientesController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:clientes.crear")]

        public async Task<IActionResult> Create(ClientesViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogError($"Error en {state.Key}: {error.ErrorMessage}");
                        }
                    }

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
                if (id != model.ClienteID)
                    return BadRequest();

                if (!ModelState.IsValid)
                {
                    model.Provincias = await ObtenerProvincias();
                    model.Ciudades = await ObtenerCiudades(model.ProvinciaID);
                    return View("Form", model);
                }

                var cliente = _mapper.Map<Cliente>(model);
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
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Details");
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

        private async Task PrepararUbicacionViewModel(ClientesViewModel viewModel)
        {
            viewModel.Provincias = await ObtenerProvincias();
            viewModel.Ciudades = await ObtenerCiudades(viewModel.ProvinciaID);
        }
    }
}
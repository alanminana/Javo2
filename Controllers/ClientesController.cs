using AutoMapper;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Clientes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Javo2.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly IClienteSearchService _searchService;
        private readonly IMapper _mapper;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(
            IClienteService clienteService,
            IMapper mapper,
            ILogger<ClientesController> logger)
        {
            _clienteService = clienteService;
            _searchService = clienteService as IClienteSearchService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
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

        [HttpGet]
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
        // Controllers/ClientesController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
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

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Edit POST");
                return View("Error");
            }
        }

        [HttpGet]
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _clienteService.DeleteClienteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DeleteConfirmed");
                return View("Error");
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
    }
}
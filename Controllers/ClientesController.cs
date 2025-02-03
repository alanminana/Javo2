// Ruta: Controllers/ClientesController.cs
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
        private readonly IMapper _mapper;
        private readonly ILogger<ClientesController> _logger;

        // Opcional, inyectar IAuditoriaService si deseas auditar desde el Controller
        private readonly IAuditoriaService? _auditoriaService;

        public ClientesController(
            IClienteService clienteService,
            IMapper mapper,
            ILogger<ClientesController> logger,
            IAuditoriaService? auditoriaService = null
        )
        {
            _clienteService = clienteService;
            _mapper = mapper;
            _logger = logger;
            _auditoriaService = auditoriaService;
        }

        // GET: Clientes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Entrando a [GET] Clientes/Index");
            var clientes = await _clienteService.GetAllClientesAsync();
            var model = _mapper.Map<IEnumerable<ClientesViewModel>>(clientes);
            return View(model);  // Views/Clientes/Index.cshtml
        }

        // GET: Clientes/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Entrando a [GET] Clientes/Create");
            var viewModel = new ClientesViewModel
            {
                Provincias = await ObtenerProvincias(),
                Ciudades = new List<SelectListItem>()
            };
            return View("_ClientesForm", viewModel); // partial o view
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientesViewModel model)
        {
            _logger.LogInformation("POST Clientes/Create => ID={ID}, ProvinciaID={Prov}, CiudadID={City}",
                model.ClienteID, model.ProvinciaID, model.CiudadID);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido: {Errors}",
                    string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                model.Provincias = await ObtenerProvincias();
                model.Ciudades = await ObtenerCiudades(model.ProvinciaID);
                return View("_ClientesForm", model);
            }

            var cliente = _mapper.Map<Cliente>(model);
            await _clienteService.CreateClienteAsync(cliente);

            // Auditoría desde Controller (opcional)
            _auditoriaService?.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = User.Identity?.Name ?? "Desconocido",
                Entidad = "Cliente",
                Accion = "Create",
                LlavePrimaria = cliente.ClienteID.ToString(),
                Detalle = $"Nombre={cliente.Nombre}, Apellido={cliente.Apellido}"
            });

            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("GET Clientes/Edit => ID={ID}", id);

            var cliente = await _clienteService.GetClienteByIDAsync(id);
            if (cliente == null)
                return NotFound();

            var viewModel = _mapper.Map<ClientesViewModel>(cliente);
            viewModel.Provincias = await ObtenerProvincias();
            viewModel.Ciudades = await ObtenerCiudades(viewModel.ProvinciaID);

            return View("_ClientesForm", viewModel);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientesViewModel model)
        {
            _logger.LogInformation("POST Clientes/Edit => ID={ID}", id);

            if (id != model.ClienteID)
                return BadRequest("ID inconsistente.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido al Editar: {Errors}",
                    string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                model.Provincias = await ObtenerProvincias();
                model.Ciudades = await ObtenerCiudades(model.ProvinciaID);
                return View("_ClientesForm", model);
            }

            var cliente = _mapper.Map<Cliente>(model);
            await _clienteService.UpdateClienteAsync(cliente);

            // Auditoría opcional
            _auditoriaService?.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = User.Identity?.Name ?? "Desconocido",
                Entidad = "Cliente",
                Accion = "Edit",
                LlavePrimaria = cliente.ClienteID.ToString(),
                Detalle = $"Nombre={cliente.Nombre}, Apellido={cliente.Apellido}"
            });

            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var cliente = await _clienteService.GetClienteByIDAsync(id);
            if (cliente == null)
                return NotFound();

            var viewModel = _mapper.Map<ClientesViewModel>(cliente);
            return View(viewModel);
        }

        // GET: Clientes/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _clienteService.GetClienteByIDAsync(id);
            if (cliente == null)
                return NotFound();

            var viewModel = _mapper.Map<ClientesViewModel>(cliente);
            return View(viewModel);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _clienteService.GetClienteByIDAsync(id);
            if (cliente == null)
                return NotFound();

            await _clienteService.DeleteClienteAsync(id);

            _auditoriaService?.RegistrarCambioAsync(new AuditoriaRegistro
            {
                FechaHora = DateTime.Now,
                Usuario = User.Identity?.Name ?? "Desconocido",
                Entidad = "Cliente",
                Accion = "Delete",
                LlavePrimaria = id.ToString(),
                Detalle = $"Eliminado cliente {cliente.Nombre} {cliente.Apellido}"
            });

            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/GetCiudades?provinciaID=..
        [HttpGet]
        public async Task<IActionResult> GetCiudades(int provinciaID)
        {
            _logger.LogInformation("GET Clientes/GetCiudades => ProvinciaID={ID}", provinciaID);

            var ciudades = await _clienteService.GetCiudadesByProvinciaAsync(provinciaID);
            var selectList = ciudades.Select(c => new SelectListItem
            {
                Value = c.CiudadID.ToString(),
                Text = c.Nombre
            }).ToList();

            _logger.LogInformation("Retornando {Count} ciudades en JSON", selectList.Count);
            return Json(selectList);
        }

        // ============================
        // Métodos auxiliares
        // ============================
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

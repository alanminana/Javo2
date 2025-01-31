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

namespace Javo2.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly IMapper _mapper;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(
            IClienteService clienteService,
            IMapper mapper,
            ILogger<ClientesController> logger)
        {
            _clienteService = clienteService;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: Clientes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Entrando a [GET] Clientes/Index");
            var clientes = await _clienteService.GetAllClientesAsync();
            var model = _mapper.Map<IEnumerable<ClientesViewModel>>(clientes);
            return View(model);
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
            return View("_ClientesForm", viewModel);
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientesViewModel model)
        {
            _logger.LogInformation("Entrando a [POST] Clientes/Create con ClienteID={ID}, ProvinciaID={Prov}, CiudadID={City}",
                model.ClienteID, model.ProvinciaID, model.CiudadID);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido al crear cliente: {Errors}",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                model.Provincias = await ObtenerProvincias();
                model.Ciudades = await ObtenerCiudades(model.ProvinciaID);
                return View("_ClientesForm", model);
            }

            var cliente = _mapper.Map<Cliente>(model);
            await _clienteService.CreateClienteAsync(cliente);

            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Entrando a [GET] Clientes/Edit con ID={ID}", id);

            var cliente = await _clienteService.GetClienteByIDAsync(id);
            if (cliente == null) return NotFound();

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
            _logger.LogInformation("Entrando a [POST] Clientes/Edit con ID={ID}", id);

            if (id != model.ClienteID) return BadRequest();

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido al editar cliente ID={ID}: {Errors}",
                    id,
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                model.Provincias = await ObtenerProvincias();
                model.Ciudades = await ObtenerCiudades(model.ProvinciaID);
                return View("_ClientesForm", model);
            }

            var cliente = _mapper.Map<Cliente>(model);
            await _clienteService.UpdateClienteAsync(cliente);
            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var cliente = await _clienteService.GetClienteByIDAsync(id);
            if (cliente == null) return NotFound();

            var viewModel = _mapper.Map<ClientesViewModel>(cliente);
            return View(viewModel);
        }

        // GET: Clientes/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _clienteService.GetClienteByIDAsync(id);
            if (cliente == null) return NotFound();

            var viewModel = _mapper.Map<ClientesViewModel>(cliente);
            return View(viewModel);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clienteService.DeleteClienteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/GetCiudades
        [HttpGet]
        public async Task<IActionResult> GetCiudades(int provinciaID)
        {
            _logger.LogInformation("Entrando a [GET] Clientes/GetCiudades con ProvinciaID={ID}", provinciaID);

            var ciudades = await _clienteService.GetCiudadesByProvinciaAsync(provinciaID);
            var selectList = new List<SelectListItem>();

            foreach (var c in ciudades)
            {
                selectList.Add(new SelectListItem
                {
                    Value = c.CiudadID.ToString(),
                    Text = c.Nombre
                });
            }
            _logger.LogInformation("Retornando {Count} ciudades en formato JSON", selectList.Count);
            return Json(selectList);
        }

        // Métodos auxiliares
        private async Task<IEnumerable<SelectListItem>> ObtenerProvincias()
        {
            var provincias = await _clienteService.GetProvinciasAsync();
            var selectList = new List<SelectListItem>();
            foreach (var p in provincias)
            {
                selectList.Add(new SelectListItem
                {
                    Value = p.ProvinciaID.ToString(),
                    Text = p.Nombre
                });
            }
            _logger.LogInformation("Obtenidas {Count} provincias del service", selectList.Count);
            return selectList;
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCiudades(int provinciaID)
        {
            var ciudades = await _clienteService.GetCiudadesByProvinciaAsync(provinciaID);
            var selectList = new List<SelectListItem>();
            foreach (var c in ciudades)
            {
                selectList.Add(new SelectListItem
                {
                    Value = c.CiudadID.ToString(),
                    Text = c.Nombre
                });
            }
            _logger.LogInformation("Obtenidas {Count} ciudades para ProvinciaID={provinciaID}", selectList.Count, provinciaID);
            return selectList;
        }
    }
}

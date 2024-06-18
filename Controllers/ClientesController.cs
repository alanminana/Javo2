using Microsoft.AspNetCore.Mvc;
using Javo2.Services;
using Javo2.ViewModels.Operaciones.Clientes;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using AutoMapper;
using System.Collections.Generic;

namespace Javo2.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<ClientesController> _logger;
        private readonly IProvinciaService _provinciaService;
        private readonly IMapper _mapper;

        public ClientesController(IClienteService clienteService, IProvinciaService provinciaService, IMapper mapper, ILogger<ClientesController> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
            _provinciaService = provinciaService;
            _mapper = mapper;
        }

        private void LogModelStateErrors()
        {
            foreach (var state in ModelState)
            {
                var key = state.Key;
                var errors = state.Value.Errors.Select(e => e.ErrorMessage).ToArray();
                _logger.LogError("ModelState Error for {Key}: {Errors}", key, string.Join(", ", errors));
            }
        }

        private void PopulateDropdowns(ClientesViewModel model)
        {
            model.Provincias = _provinciaService.GetAllProvincias().Select(p => new SelectListItem
            {
                Value = p.ProvinciaID.ToString(),
                Text = p.Nombre
            }).ToList();

            model.Ciudades = model.ProvinciaID > 0
                ? _provinciaService.GetCiudadesByProvinciaId(model.ProvinciaID).Select(c => new SelectListItem
                {
                    Value = c.CiudadID.ToString(),
                    Text = c.Nombre
                }).ToList()
                : new List<SelectListItem>();
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called");
            var clientes = await _clienteService.GetAllClientesAsync();
            _logger.LogInformation("Clientes retrieved: {ClientesCount}", clientes.Count());
            return View(clientes);
        }

        public async Task<IActionResult> Filter(ClienteFilterDto filters)
        {
            _logger.LogInformation("Filter action called with filters: {Filters}", filters);
            var clientes = await _clienteService.FilterClientesAsync(filters);
            return PartialView("_ClientesTable", clientes);
        }

        public IActionResult Create()
        {
            var model = new ClientesViewModel
            {
                ModificadoPor = "cosmefulanito",
                Provincias = _provinciaService.GetAllProvincias().Select(p => new SelectListItem
                {
                    Value = p.ProvinciaID.ToString(),
                    Text = p.Nombre
                }).ToList(),
                Ciudades = new List<SelectListItem>()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientesViewModel model)
        {
            _logger.LogInformation("Create POST action called with Cliente: {Cliente}", model.Nombre);

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                PopulateDropdowns(model);
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(model);
            }

            await _clienteService.CreateClienteAsync(model);
            _logger.LogInformation("Cliente created successfully");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Edit GET action called with ID: {Id}", id);
            var cliente = await _clienteService.GetClienteByIdAsync(id);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente with ID {Id} not found", id);
                return NotFound();
            }

            var model = _mapper.Map<ClientesViewModel>(cliente);
            PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientesViewModel model)
        {
            _logger.LogInformation("Edit POST action called with Cliente: {Cliente}", model.Nombre);

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                PopulateDropdowns(model);
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(model);
            }

            await _clienteService.UpdateClienteAsync(model);
            _logger.LogInformation("Cliente updated successfully");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Delete GET action called with ID: {Id}", id);
            var cliente = await _clienteService.GetClienteByIdAsync(id);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente with ID {Id} not found", id);
                return NotFound();
            }
            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Delete POST action called with ID: {Id}", id);
            await _clienteService.DeleteClienteAsync(id);
            _logger.LogInformation("Cliente deleted successfully");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Details action called with ID: {Id}", id);
            var cliente = await _clienteService.GetClienteByIdAsync(id);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente with ID {Id} not found", id);
                return NotFound();
            }
            return View(cliente);
        }

        [HttpGet]
        public IActionResult GetCiudades(int provinciaId)
        {
            var ciudades = _provinciaService.GetCiudadesByProvinciaId(provinciaId);
            return Json(ciudades.Select(c => new { c.CiudadID, c.Nombre }));
        }
    }
}

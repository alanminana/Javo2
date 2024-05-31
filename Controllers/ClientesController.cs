using Microsoft.AspNetCore.Mvc;
using javo2.Services;
using javo2.ViewModels.Operaciones.Clientes;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace javo2.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(IClienteService clienteService, ILogger<ClientesController> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called");
            var clientes = await _clienteService.GetAllClientesAsync();
            _logger.LogInformation("Clientes retrieved: {ClientesCount}", clientes.Count());
            return View(clientes);
        }

        public IActionResult Create()
        {
            var model = new ClientesViewModel
            {
                ModificadoPor = "cosmefulanito"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientesViewModel model)
        {
            _logger.LogInformation("Create POST action called with Cliente: {Cliente}", model.Nombre);
            if (ModelState.IsValid)
            {
                await _clienteService.CreateClienteAsync(model);
                _logger.LogInformation("Cliente created successfully");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return View(model);
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
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientesViewModel model)
        {
            _logger.LogInformation("Edit POST action called with Cliente: {Cliente}", model.Nombre);
            if (ModelState.IsValid)
            {
                await _clienteService.UpdateClienteAsync(model);
                _logger.LogInformation("Cliente updated successfully");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return View(model);
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
    }
}

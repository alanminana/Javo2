// Controllers/ClientesController.cs
using Microsoft.AspNetCore.Mvc;
using Javo2.ViewModels.Operaciones.Clientes;
using AutoMapper;
using Javo2.Controllers.Base;
using Microsoft.AspNetCore.Mvc.Rendering;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.Helpers;

namespace Javo2.Controllers
{
    public class ClientesController : BaseController
    {
        private readonly IClienteService _clienteService;
        private readonly IMapper _mapper;
        private readonly IDropdownService _dropdownService;
        private readonly IProvinciaService _provinciaService;

        public ClientesController(
            IClienteService clienteService,
            IMapper mapper,
            IDropdownService dropdownService,
            IProvinciaService provinciaService,
            ILogger<ClientesController> logger)
            : base(logger)
        {
            _clienteService = clienteService;
            _mapper = mapper;
            _dropdownService = dropdownService;
            _provinciaService = provinciaService;
        }


        /// <summary>
        /// Pobla las listas desplegables de Provincias y Ciudades en el ViewModel.
        /// </summary>
        /// <returns>Tarea asincrónica.</returns>
        private async Task PopulateDropdownsAsync(ClientesViewModel model)
        {
            var provincias = await _dropdownService.GetProvinciasAsync();
            model.Provincias = provincias.Select(p => new SelectListItem
            {
                Value = p.ProvinciaID.ToString(),
                Text = p.Nombre
            }).ToList();

            if (model.ProvinciaID > 0)
            {
                var ciudades = await _dropdownService.GetCiudadesByProvinciaIdAsync(model.ProvinciaID);
                model.Ciudades = ciudades.Select(c => new SelectListItem
                {
                    Value = c.CiudadID.ToString(),
                    Text = c.Nombre
                }).ToList();
            }
            else
            {
                model.Ciudades = new List<SelectListItem>();
            }
        }



        public async Task<IActionResult> Index(string? filtroValor, string? filtroTipo)
        {
            var clientes = await _clienteService.GetAllClientesAsync();

            if (!string.IsNullOrEmpty(filtroValor) && !string.IsNullOrEmpty(filtroTipo))
            {
                clientes = filtroTipo switch
                {
                    "Nombre" => clientes.Where(c => c.Nombre.Contains(filtroValor, StringComparison.OrdinalIgnoreCase)),
                    "Apellido" => clientes.Where(c => c.Apellido.Contains(filtroValor, StringComparison.OrdinalIgnoreCase)),
                    "DNI" => clientes.Where(c => c.DNI.ToString().Contains(filtroValor)),
                    "Email" => clientes.Where(c => c.Email.Contains(filtroValor, StringComparison.OrdinalIgnoreCase)),
                    _ => clientes
                };
            }

            var model = _mapper.Map<IEnumerable<ClientesViewModel>>(clientes);
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var model = new ClientesViewModel
            {
                ModificadoPor = User.Identity?.Name ?? "UsuarioDesconocido"
            };
            await PopulateDropdownsAsync(model);
            return View("Form", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientesViewModel model)
        {
            // Establecer ModificadoPor y remover del ModelState
            model.ModificadoPor = User.Identity?.Name ?? "UsuarioDesconocido";
            ModelState.Remove(nameof(model.ModificadoPor));

            _logger.LogInformation("Intentando crear un cliente con los siguientes datos: {@Model}", model);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState no es válido. Errores: {@Errors}", ModelState.Values.SelectMany(v => v.Errors));
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            try
            {
                var cliente = _mapper.Map<Cliente>(model);
                _logger.LogInformation("Cliente mapeado: {@Cliente}", cliente);

                await _clienteService.CreateClienteAsync(cliente);

                _logger.LogInformation("Cliente creado exitosamente con ID: {ClienteID}", cliente.ClienteID);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el Cliente: {ErrorMessage}", ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _clienteService.GetClienteByIdAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<ClientesViewModel>(cliente);
            await PopulateDropdownsAsync(model);
            return View("Form", model); // Cambiado a "Form"
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientesViewModel model)
        {
            // Establecer el valor de ModificadoPor
            model.ModificadoPor = User.Identity?.Name ?? "UsuarioDesconocido";

            // Remover el estado del modelo para ModificadoPor
            ModelState.Remove(nameof(model.ModificadoPor));

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            try
            {
                var cliente = _mapper.Map<Cliente>(model);
                await _clienteService.UpdateClienteAsync(cliente);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is KeyNotFoundException)
            {
                _logger.LogError(ex, "Error al actualizar el Cliente: {ErrorMessage}", ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }
        }


        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _clienteService.GetClienteByIdAsync(id);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente con ID {Id} no encontrado", id);
                return NotFound();
            }
            var model = _mapper.Map<ClientesViewModel>(cliente);
            return View(model);
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
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error al eliminar el Cliente: {Error}", ex.Message);
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error al eliminar el Cliente: {Error}", ex.Message);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar el cliente.");
                return View();
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetCiudades(int provinciaId)
        {
            var ciudades = await _dropdownService.GetCiudadesByProvinciaIdAsync(provinciaId);
            var ciudadesList = ciudades.Select(c => new SelectListItem
            {
                Value = c.CiudadID.ToString(),
                Text = c.Nombre
            }).ToList();
            return Json(ciudadesList);
        }

        public async Task<IActionResult> Details(int id)
        {
            var cliente = await _clienteService.GetClienteByIdAsync(id);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente con ID {Id} no encontrado", id);
                return NotFound();
            }
            var model = _mapper.Map<ClientesViewModel>(cliente);
            return View(model);
        }
    }
}

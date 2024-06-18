using Microsoft.AspNetCore.Mvc;
using Javo2.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using Javo2.ViewModels.Operaciones.Ventas;
using Javo2.IServices;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Javo2.Controllers
{
    public class VentasController : Controller
    {
        private readonly IVentaService _ventaService;
        private readonly IClienteService _clienteService;
        private readonly IProductoService _productoService;
        private readonly ILogger<VentasController> _logger;

        public VentasController(IVentaService ventaService, IClienteService clienteService, IProductoService productoService, ILogger<VentasController> logger)
        {
            _ventaService = ventaService;
            _clienteService = clienteService;
            _productoService = productoService;
            _logger = logger;
        }

        // Acción para mostrar la lista de ventas filtradas por fecha
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin)
        {
            _logger.LogInformation("Index action called with fechaInicio: {FechaInicio}, fechaFin: {FechaFin}", fechaInicio, fechaFin);
            var ventas = await _ventaService.GetVentasByFechaAsync(fechaInicio, fechaFin);
            _logger.LogInformation("Ventas retrieved: {VentasCount}", ventas.Count());
            return View(ventas);
        }

        // Acción GET para mostrar el formulario de creación de ventas
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Create GET action called");
            var model = new VentasViewModel
            {
                FechaVenta = DateTime.Now,
                NumeroFactura = GenerateNumeroFactura(),
                Usuario = "cosmefulanito",
                Vendedor = "cosmefulanito"
            };
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        // Método para generar un número de factura (Ejemplo)
        private string GenerateNumeroFactura()
        {
            return "F001"; // Ejemplo: lógica estática para propósitos de demostración
        }

        // Acción POST para manejar la creación de una nueva venta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentasViewModel model)
        {
            _logger.LogInformation("Create POST action called with Venta: {Venta}", model);
            if (ModelState.IsValid)
            {
                await _ventaService.CreateVentaAsync(model);
                _logger.LogInformation("Venta created successfully");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        // Acción GET para mostrar el formulario de edición de ventas
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Edit GET action called with ID: {Id}", id);
            var venta = await _ventaService.GetVentaByIdAsync(id);
            if (venta == null)
            {
                _logger.LogWarning("Venta with ID {Id} not found", id);
                return NotFound();
            }
            await PopulateDropdownsAsync(venta);
            return View(venta);
        }

        // Acción POST para manejar la edición de una venta existente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VentasViewModel model)
        {
            _logger.LogInformation("Edit POST action called with Venta: {Venta}", model);
            if (ModelState.IsValid)
            {
                await _ventaService.UpdateVentaAsync(model);
                _logger.LogInformation("Venta updated successfully");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        // Acción GET para mostrar la confirmación de eliminación de una venta
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Delete GET action called with ID: {Id}", id);
            var venta = await _ventaService.GetVentaByIdAsync(id);
            if (venta == null)
            {
                _logger.LogWarning("Venta with ID {Id} not found", id);
                return NotFound();
            }
            return View(venta);
        }

        // Acción POST para manejar la eliminación de una venta
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Delete POST action called with ID: {Id}", id);
            await _ventaService.DeleteVentaAsync(id);
            _logger.LogInformation("Venta deleted successfully");
            return RedirectToAction(nameof(Index));
        }

        // Acción GET para mostrar los detalles de una venta
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Details action called with ID: {Id}", id);
            var venta = await _ventaService.GetVentaByIdAsync(id);
            if (venta == null)
            {
                _logger.LogWarning("Venta with ID {Id} not found", id);
                return NotFound();
            }
            return View(venta);
        }

        // Acción POST para buscar un cliente por su DNI
        [HttpPost]
        public async Task<JsonResult> BuscarClientePorDNI(int dni)
        {
            _logger.LogInformation("BuscarClientePorDNI action called with DNI: {Dni}", dni);
            var cliente = await _clienteService.GetClienteByDniAsync(dni);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente with DNI {Dni} not found", dni);
                return Json(new { success = false, message = "Cliente no encontrado" });
            }
            return Json(new
            {
                success = true,
                data = new
                {
                    cliente.ClienteID,
                    nombre = cliente.Nombre,
                    telefono = cliente.Telefono,
                    calle = cliente.Calle,
                    localidad = cliente.Localidad,
                    celular = cliente.Celular,
                    limiteCredito = cliente.ImporteCredito,
                    saldo = cliente.Saldo,
                    saldoDisponible = cliente.DeudaTotal
                }
            });
        }

        // Acción POST para buscar un producto por su código
        [HttpPost]
        public async Task<JsonResult> BuscarProducto(string codigoProducto)
        {
            _logger.LogInformation("BuscarProducto action called with codigoProducto: {CodigoProducto}", codigoProducto);
            var producto = await _productoService.GetProductoByCodigoAsync(codigoProducto);
            if (producto == null)
            {
                _logger.LogWarning("Producto with codigoProducto {CodigoProducto} not found", codigoProducto);
                return Json(new { success = false, message = "Producto no encontrado" });
            }
            return Json(new
            {
                success = true,
                data = new
                {
                    producto.ProductoID,
                    producto.ProductoIDAlfa,
                    producto.CodBarra,
                    nombreProducto = producto.Nombre,
                    descripcion1Producto = producto.Descripcion,
                    producto.PLista,
                    producto.PCosto
                }
            });
        }

        // Acción POST para buscar un producto por su nombre
        [HttpPost]
        public async Task<JsonResult> BuscarProductoPorNombre(string nombreProducto)
        {
            _logger.LogInformation("BuscarProductoPorNombre action called with nombreProducto: {NombreProducto}", nombreProducto);
            var producto = await _productoService.GetProductoByNombreAsync(nombreProducto);
            if (producto == null)
            {
                _logger.LogWarning("Producto with nombreProducto {NombreProducto} not found", nombreProducto);
                return Json(new { success = false, message = "Producto no encontrado" });
            }
            return Json(new
            {
                success = true,
                data = new
                {
                    producto.ProductoID,
                    producto.ProductoIDAlfa,
                    producto.CodBarra,
                    nombreProducto = producto.Nombre,
                    descripcion1Producto = producto.Descripcion,
                    producto.PLista,
                    producto.PCosto
                }
            });
        }

        // Acción POST para buscar productos por rubro
        [HttpPost]
        public async Task<JsonResult> BuscarProductosPorRubro(string rubroProducto)
        {
            _logger.LogInformation("BuscarProductosPorRubro action called with rubroProducto: {RubroProducto}", rubroProducto);
            var productos = await _productoService.GetProductosByRubroAsync(rubroProducto);
            if (!productos.Any())
            {
                _logger.LogWarning("No products found for rubroProducto {RubroProducto}", rubroProducto);
                return Json(new { success = false, message = "No se encontraron productos en este rubro" });
            }
            return Json(new
            {
                success = true,
                data = productos.Select(p => new
                {
                    p.ProductoID,
                    p.ProductoIDAlfa,
                    p.CodBarra,
                    nombreProducto = p.Nombre,
                    descripcion1Producto = p.Descripcion,
                    p.PLista,
                    p.PCosto
                })
            });
        }

        // Acción GET para autocompletar rubros
        [HttpGet]
        public async Task<JsonResult> AutocompleteRubro(string term)
        {
            _logger.LogInformation("AutocompleteRubro action called with term: {Term}", term);
            var rubros = await _productoService.GetRubrosAutocompleteAsync(term);
            return Json(rubros);
        }


        // Acción GET para autocompletar marcas
        [HttpGet]
        public async Task<JsonResult> AutocompleteMarca(string term)
        {
            _logger.LogInformation("AutocompleteMarca action called with term: {Term}", term);
            var marcas = await _productoService.GetMarcasAutocompleteAsync(term);
            return Json(marcas);
        }

        // Acción para mostrar la lista de ventas pendientes de entrega
        public async Task<IActionResult> EntregaProductos()
        {
            _logger.LogInformation("EntregaProductos action called");
            var ventas = await _ventaService.GetVentasByEstadoAsync(EstadoVenta.PendienteDeEntrega);
            return View(ventas);
        }

        // Acción para mostrar la lista de ventas pendientes de autorización
        public async Task<IActionResult> Autorizaciones()
        {
            _logger.LogInformation("Autorizaciones action called");
            var ventas = await _ventaService.GetVentasByEstadoAsync(EstadoVenta.PendienteDeAutorizacion);
            return View(ventas);
        }

        // Acción POST para aprobar una venta pendiente de autorización
        [HttpPost]
        public async Task<IActionResult> Aprobar(int id)
        {
            _logger.LogInformation("Aprobar action called with ID: {Id}", id);
            await _ventaService.UpdateEstadoVentaAsync(id, EstadoVenta.PendienteDeEntrega);
            return RedirectToAction(nameof(Autorizaciones));
        }

        // Acción POST para rechazar una venta pendiente de autorización
        [HttpPost]
        public async Task<IActionResult> Rechazar(int id)
        {
            _logger.LogInformation("Rechazar action called with ID: {Id}", id);
            await _ventaService.UpdateEstadoVentaAsync(id, EstadoVenta.Rechazada);
            return RedirectToAction(nameof(Autorizaciones));
        }

        // Método para poblar los dropdowns en el modelo de vista de ventas
        private static async Task PopulateDropdownsAsync(VentasViewModel model)
        {
            model.FormasPago = await Task.FromResult(new List<SelectListItem>
            {
                new() { Text = "Tarjeta de crédito", Value = "Tarjeta de crédito" },
                new() { Text = "Efectivo", Value = "Efectivo" },
                new() { Text = "Transferencia", Value = "Transferencia" },
                new() { Text = "Pago Virtual", Value = "Pago Virtual" },
                new() { Text = "Débito", Value = "Débito" },
                new() { Text = "Crédito Personal", Value = "Crédito Personal" }
            });

            model.Bancos = await Task.FromResult(new List<SelectListItem>
            {
                new() { Text = "HSBC", Value = "HSBC" },
                new() { Text = "BBVA", Value = "BBVA" },
                new() { Text = "Santander", Value = "Santander" }
            });

            model.TipoTarjeta = await Task.FromResult(new List<SelectListItem>
            {
                new() { Text = "AMEX", Value = "AMEX" },
                new() { Text = "VISA", Value = "VISA" },
                new() { Text = "MASTERCARD", Value = "MASTERCARD" }
            });

            model.Cuotas = await Task.FromResult(new List<SelectListItem>
            {
                new() { Text = "1 cuota", Value = "1" },
                new() { Text = "3 cuotas", Value = "3" },
                new() { Text = "6 cuotas", Value = "6" },
                new() { Text = "12 cuotas", Value = "12" }
            });

            model.EntidadesElectronicas = await Task.FromResult(new List<SelectListItem>
            {
                new() { Text = "Personal Pay", Value = "Personal Pay" },
                new() { Text = "Mercado Pago", Value = "Mercado Pago" }
            });

            model.PlanesFinanciamiento = await Task.FromResult(new List<SelectListItem>
            {
                new() { Text = "Ahora 12", Value = "Ahora 12" },
                new() { Text = "Plan 18", Value = "Plan 18" }
            });

            model.TipoEntregas = await Task.FromResult(new List<SelectListItem>
            {
                new() { Text = "Envío a domicilio", Value = "Envío a domicilio" },
                new() { Text = "Retiro en tienda", Value = "Retiro en tienda" }
            });
        }
    }
}

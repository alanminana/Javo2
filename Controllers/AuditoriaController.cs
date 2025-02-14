// File: Controllers/AuditoriaController.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Javo2.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly IAuditoriaService _auditoriaService;
        private readonly IProductoService _productoService; // Para revertir subida de precios
        private readonly ILogger<AuditoriaController> _logger;
        private readonly IVentaService _ventaService;

        public AuditoriaController(
            IAuditoriaService auditoriaService,
            IProductoService productoService,
            ILogger<AuditoriaController> logger,
            IVentaService ventaService)
        {
            _auditoriaService = auditoriaService;
            _productoService = productoService;
            _logger = logger;
            _ventaService = ventaService;
        }

        public async Task<IActionResult> Index()
        {
            var registros = await _auditoriaService.GetAllRegistrosAsync();
            var lista = registros.OrderByDescending(r => r.FechaHora).ToList();
            return View(lista); // Views\Auditoria\Index.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> Rollback(int id)
        {
            _logger.LogInformation("Rollback invoked for auditoriaID={id}", id);

            var reg = await _auditoriaService.GetRegistroByIDAsync(id);
            if (reg == null)
            {
                TempData["Error"] = $"No se encontró el registro de auditoría con ID {id}.";
                return RedirectToAction(nameof(Index));
            }

            if (reg.EsRevertido)
            {
                TempData["Error"] = "Este registro ya fue revertido.";
                return RedirectToAction(nameof(Index));
            }

            // Ejemplo de rollback para Venta "Create"
            if (reg.Entidad == "Venta" && reg.Accion == "Create")
            {
                if (int.TryParse(reg.LlavePrimaria, out int VentaID))
                {
                    var venta = await _ventaService.GetVentaByIDAsync(VentaID);
                    if (venta != null)
                    {
                        // Asumimos que el rollback de una creación es eliminar la venta
                        await _ventaService.DeleteVentaAsync(VentaID);
                        reg.EsRevertido = true;
                        reg.RollbackUser = User.Identity?.Name ?? "Desconocido";
                        reg.RollbackFecha = DateTime.Now;
                        TempData["Success"] = $"Se revirtió la creación de la Venta {VentaID}";
                    }
                    else
                    {
                        TempData["Error"] = "No se encontró la Venta para revertir su creación";
                    }
                }
                else
                {
                    TempData["Error"] = "Llave primaria de la venta no es válida.";
                }
            }
            // Ejemplo de rollback para Producto "UpdatePrices"
            else if (reg.Entidad == "Producto" && reg.Accion == "UpdatePrices")
            {
                var partes = reg.Detalle.Split('|', StringSplitOptions.RemoveEmptyEntries);
                foreach (var parte in partes)
                {
                    // Formato esperado: "101:PCosto=100->110;PContado=150->165;PLista=180->198"
                    var sub = parte.Split(':');
                    if (sub.Length < 2) continue;
                    if (!int.TryParse(sub[0], out int prodID))
                    {
                        _logger.LogWarning("No se pudo parsear el ID de producto en rollback: {Parte}", parte);
                        continue;
                    }
                    var campos = sub[1].Split(';');
                    var producto = await _productoService.GetProductoByIDAsync(prodID);
                    if (producto == null)
                    {
                        _logger.LogWarning("Producto ID {prodID} no encontrado, se omite en rollback", prodID);
                        continue;
                    }

                    foreach (var c in campos)
                    {
                        var eq = c.Split('=');
                        if (eq.Length < 2) continue;
                        var campo = eq[0]; // "PCosto", etc.
                        var vals = eq[1].Split("->"); // ["100", "110"]
                        if (vals.Length < 1) continue;
                        if (!decimal.TryParse(vals[0], out decimal antes))
                        {
                            _logger.LogWarning("No se pudo parsear el valor anterior en rollback para producto ID {prodID}", prodID);
                            continue;
                        }

                        if (campo == "PCosto")
                            producto.PCosto = antes;
                        else if (campo == "PContado")
                            producto.PContado = antes;
                        else if (campo == "PLista")
                            producto.PLista = antes;
                    }
                    await _productoService.UpdateProductoAsync(producto);
                }

                reg.EsRevertido = true;
                reg.RollbackUser = User.Identity?.Name ?? "Desconocido";
                reg.RollbackFecha = DateTime.Now;
                TempData["Success"] = $"Rollback realizado para auditoría ID {id}.";
            }
            else
            {
                TempData["Error"] = "Este tipo de registro no admite rollback automático.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

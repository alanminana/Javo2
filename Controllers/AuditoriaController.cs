// Archivo: Controllers/AuditoriaController.cs
using Javo2.IServices;
using Javo2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Linq;
using Javo2.Services;

namespace Javo2.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly IAuditoriaService _auditoriaService;
        private readonly IProductoService _productoService; // Para revertir suba precios
        private readonly ILogger<AuditoriaController> _logger;
        private readonly IVentaService _ventaService;

        public AuditoriaController(
            IAuditoriaService auditoriaService,
            IProductoService productoService,
            ILogger<AuditoriaController> logger,
                        IVentaService ventaService            // <--- INYECTAR

        )
        {
            _auditoriaService = auditoriaService;
            _productoService = productoService;
            _logger = logger;
            _ventaService = ventaService;           // <--- ASIGNAR

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

            if (reg.Entidad == "Venta" && reg.Accion == "Create")
            {
                // parse: "Cliente=..., Total=..., Estado=..." -> No lo necesitamos mucho
                // 1) Buscar la venta en _ventaService
                var VentaID = int.Parse(reg.LlavePrimaria);
                var venta = await _ventaService.GetVentaByIDAsync(VentaID);
                if (venta != null)
                {
                    // El rollback de "create" = "Delete"? 
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
            if (reg.Entidad == "Producto" && reg.Accion == "UpdatePrices")
            {
                var partes = reg.Detalle.Split('|', StringSplitOptions.RemoveEmptyEntries);
                foreach (var parte in partes)
                {
                    // "101:PCosto=100->110;PContado=150->165;PLista=180->198"
                    var sub = parte.Split(':');
                    int prodID = int.Parse(sub[0]);
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
                        var campo = eq[0]; // "PCosto"
                        var vals = eq[1].Split("->"); // ["100", "110"]
                        var antes = decimal.Parse(vals[0]);

                        if (campo == "PCosto") producto.PCosto = antes;
                        else if (campo == "PContado") producto.PContado = antes;
                        else if (campo == "PLista") producto.PLista = antes;
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

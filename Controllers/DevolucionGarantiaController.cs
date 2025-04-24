// Controllers/DevolucionGarantiaController.cs
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.DevolucionGarantia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using System.ComponentModel.DataAnnotations;

namespace Javo2.Controllers
{
    public class DevolucionGarantiaController : BaseController
    {
        private readonly IDevolucionGarantiaService _devolucionService;
        private readonly IVentaService _ventaService;
        private readonly IProductoService _productoService;
        private readonly IStockService _stockService;
        private readonly IMapper _mapper;

        public DevolucionGarantiaController(
            IDevolucionGarantiaService devolucionService,
            IVentaService ventaService,
            IProductoService productoService,
            IStockService stockService,
            IMapper mapper,
            ILogger<DevolucionGarantiaController> logger)
            : base(logger)
        {
            _devolucionService = devolucionService;
            _ventaService = ventaService;
            _productoService = productoService;
            _stockService = stockService;
            _mapper = mapper;
        }

        // GET: DevolucionGarantia
        public async Task<IActionResult> Index()
        {
            try
            {
                var devoluciones = await _devolucionService.GetAllAsync();
                var model = devoluciones.Select(d => new DevolucionGarantiaListViewModel
                {
                    DevolucionGarantiaID = d.DevolucionGarantiaID,
                    VentaID = d.VentaID,
                    NumeroFactura = d.Venta?.NumeroFactura ?? $"Venta #{d.VentaID}",
                    NombreCliente = d.NombreCliente,
                    FechaSolicitud = d.FechaSolicitud,
                    TipoCaso = d.TipoCaso,
                    Estado = d.Estado,
                    CantidadProductos = d.Items.Sum(i => i.Cantidad),
                    FechaResolucion = d.FechaResolucion
                }).OrderByDescending(d => d.FechaSolicitud).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el listado de devoluciones/garantías");
                return View("Error");
            }
        }

        // GET: DevolucionGarantia/Create
        // GET: DevolucionGarantia/Create
        public IActionResult Create()
        {
            var model = new DevolucionGarantiaViewModel
            {
                FechaSolicitud = DateTime.Now,
                TiposCaso = GetTiposCasoSelectList(),
                Motivos = GetMotivosSelectList()
            };
            // antes: return View(model);
            return View("Create", model);
        }

        // POST: DevolucionGarantia/BuscarVenta
        [HttpPost]
        public async Task<IActionResult> BuscarVenta(string numeroFactura)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(numeroFactura))
                {
                    return Json(new { success = false, message = "Ingrese un número de factura válido" });
                }

                // Buscar la venta por número de factura
                var ventas = await _ventaService.GetAllVentasAsync();
                var venta = ventas.FirstOrDefault(v => v.NumeroFactura.Contains(numeroFactura));

                if (venta == null)
                {
                    return Json(new { success = false, message = "No se encontró la venta con ese número de factura" });
                }

                // Crear el modelo de productos
                var items = venta.ProductosPresupuesto.Select(p => new ItemDevolucionGarantiaViewModel
                {
                    ProductoID = p.ProductoID,
                    CodigoAlfa = p.CodigoAlfa,
                    NombreProducto = p.NombreProducto,
                    Cantidad = p.Cantidad,
                    PrecioUnitario = p.PrecioUnitario,
                    Subtotal = p.PrecioTotal,
                    Seleccionado = false,
                    ProductoDanado = false,
                    EstadoProducto = "Funcional",
                    EstadosProducto = GetEstadosProductoSelectList()
                }).ToList();

                return Json(new
                {
                    success = true,
                    ventaID = venta.VentaID,
                    numeroFactura = venta.NumeroFactura,
                    cliente = venta.NombreCliente,
                    fecha = venta.FechaVenta.ToString("dd/MM/yyyy"),
                    items = items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar venta");
                return Json(new { success = false, message = "Error al buscar la venta: " + ex.Message });
            }
        }


        // POST: DevolucionGarantia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DevolucionGarantiaViewModel model)
        {
            model.TiposCaso = GetTiposCasoSelectList();
            model.Motivos = GetMotivosSelectList();

            if (model.Items == null || !model.Items.Any(i => i.Seleccionado))
                ModelState.AddModelError("", "Debe seleccionar al menos un producto");

            if (!ModelState.IsValid)
                return View("Form", model);

            var venta = await _ventaService.GetVentaByIDAsync(model.VentaID);
            if (venta == null)
            {
                ModelState.AddModelError("", "La venta seleccionada no existe");
                return View("Form", model);
            }

            var devolucion = new DevolucionGarantia
            {
                VentaID = model.VentaID,
                NombreCliente = venta.NombreCliente,
                FechaSolicitud = model.FechaSolicitud,
                TipoCaso = model.TipoCaso,
                Motivo = model.Motivo,
                Descripcion = model.Descripcion,
                Estado = EstadoCaso.Pendiente,
                Usuario = User.Identity?.Name ?? "Sistema",
                Items = model.Items
                    .Where(i => i.Seleccionado)
                    .Select(i => new ItemDevolucionGarantia
                    {
                        ProductoID = i.ProductoID,
                        NombreProducto = i.NombreProducto,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.PrecioUnitario,
                        ProductoDanado = i.ProductoDanado,
                        EstadoProducto = i.EstadoProducto
                    }).ToList()
            };

            if (model.TipoCaso == TipoCaso.Cambio && model.CambiosProducto?.Any() == true)
            {
                devolucion.CambiosProducto = model.CambiosProducto
                    .Select(c => new CambioProducto
                    {
                        ProductoOriginalID = c.ProductoOriginalID,
                        NombreProductoOriginal = c.NombreProductoOriginal,
                        ProductoNuevoID = c.ProductoNuevoID,
                        NombreProductoNuevo = c.NombreProductoNuevo,
                        Cantidad = c.Cantidad,
                        DiferenciaPrecio = c.DiferenciaPrecio
                    }).ToList();
            }

            var id = await _devolucionService.CreateAsync(devolucion);
            TempData["Success"] = $"{model.TipoCaso} registrada con éxito, ID: {id}";
            return RedirectToAction(nameof(Details), new { id });
        }


        // GET: DevolucionGarantia/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var devolucion = await _devolucionService.GetByIDAsync(id);
                if (devolucion == null)
                {
                    return NotFound();
                }

                var venta = await _ventaService.GetVentaByIDAsync(devolucion.VentaID);

                var model = new DevolucionGarantiaViewModel
                {
                    DevolucionGarantiaID = devolucion.DevolucionGarantiaID,
                    VentaID = devolucion.VentaID,
                    NumeroFactura = venta?.NumeroFactura ?? $"Venta #{devolucion.VentaID}",
                    NombreCliente = devolucion.NombreCliente,
                    FechaVenta = venta?.FechaVenta ?? DateTime.Now,
                    FechaSolicitud = devolucion.FechaSolicitud,
                    TipoCaso = devolucion.TipoCaso,
                    Motivo = devolucion.Motivo,
                    Descripcion = devolucion.Descripcion,
                    Estado = devolucion.Estado,
                    Comentarios = devolucion.Comentarios,
                    FechaResolucion = devolucion.FechaResolucion,
                    Items = devolucion.Items.Select(i => new ItemDevolucionGarantiaViewModel
                    {
                        ItemDevolucionGarantiaID = i.ItemDevolucionGarantiaID,
                        ProductoID = i.ProductoID,
                        NombreProducto = i.NombreProducto,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.PrecioUnitario,
                        Subtotal = i.Cantidad * i.PrecioUnitario,
                        Seleccionado = true,
                        ProductoDanado = i.ProductoDanado,
                        EstadoProducto = i.EstadoProducto
                    }).ToList(),
                    CambiosProducto = devolucion.CambiosProducto.Select(c => new CambioProductoViewModel
                    {
                        CambioProductoID = c.CambioProductoID,
                        ProductoOriginalID = c.ProductoOriginalID,
                        NombreProductoOriginal = c.NombreProductoOriginal,
                        ProductoNuevoID = c.ProductoNuevoID,
                        NombreProductoNuevo = c.NombreProductoNuevo,
                        Cantidad = c.Cantidad,
                        DiferenciaPrecio = c.DiferenciaPrecio
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al mostrar detalles de devolución/garantía");
                return View("Error");
            }
        }

        // GET: DevolucionGarantia/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var devolucion = await _devolucionService.GetByIDAsync(id);
                if (devolucion == null)
                {
                    return NotFound();
                }

                // Solo permitir editar si está pendiente
                if (devolucion.Estado != EstadoCaso.Pendiente)
                {
                    TempData["Warning"] = "Solo se pueden editar devoluciones/garantías en estado pendiente";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var venta = await _ventaService.GetVentaByIDAsync(devolucion.VentaID);

                var model = new DevolucionGarantiaViewModel
                {
                    DevolucionGarantiaID = devolucion.DevolucionGarantiaID,
                    VentaID = devolucion.VentaID,
                    NumeroFactura = venta?.NumeroFactura ?? $"Venta #{devolucion.VentaID}",
                    NombreCliente = devolucion.NombreCliente,
                    FechaVenta = venta?.FechaVenta ?? DateTime.Now,
                    FechaSolicitud = devolucion.FechaSolicitud,
                    TipoCaso = devolucion.TipoCaso,
                    TiposCaso = GetTiposCasoSelectList(),
                    Motivo = devolucion.Motivo,
                    Motivos = GetMotivosSelectList(),
                    Descripcion = devolucion.Descripcion,
                    Estado = devolucion.Estado,
                    Comentarios = devolucion.Comentarios,
                    Items = devolucion.Items.Select(i => new ItemDevolucionGarantiaViewModel
                    {
                        ItemDevolucionGarantiaID = i.ItemDevolucionGarantiaID,
                        ProductoID = i.ProductoID,
                        NombreProducto = i.NombreProducto,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.PrecioUnitario,
                        Subtotal = i.Cantidad * i.PrecioUnitario,
                        Seleccionado = true,
                        ProductoDanado = i.ProductoDanado,
                        EstadoProducto = i.EstadoProducto,
                        EstadosProducto = GetEstadosProductoSelectList()
                    }).ToList(),
                    // Update the lambda expression to be asynchronous
                    CambiosProducto = devolucion.CambiosProducto.Select(async c => new CambioProductoViewModel
                    {
                        CambioProductoID = c.CambioProductoID,
                        ProductoOriginalID = c.ProductoOriginalID,
                        NombreProductoOriginal = c.NombreProductoOriginal,
                        ProductoNuevoID = c.ProductoNuevoID,
                        NombreProductoNuevo = c.NombreProductoNuevo,
                        Cantidad = c.Cantidad,
                        DiferenciaPrecio = c.DiferenciaPrecio,
                        ProductosDisponibles = await GetProductosSelectList()
                    }).Select(t => t.Result).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar devolución/garantía");
                return View("Error");
            }
        }

        // POST: DevolucionGarantia/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DevolucionGarantiaViewModel model)
        {
            if (id != model.DevolucionGarantiaID) return NotFound();

            model.TiposCaso = GetTiposCasoSelectList();
            model.Motivos = GetMotivosSelectList();
            foreach (var i in model.Items) i.EstadosProducto = GetEstadosProductoSelectList();
            foreach (var c in model.CambiosProducto) c.ProductosDisponibles = await GetProductosSelectList();

            if (model.Items == null || !model.Items.Any(i => i.Seleccionado))
                ModelState.AddModelError("", "Debe seleccionar al menos un producto");

            if (!ModelState.IsValid)
                return View("Form", model);

            var d = await _devolucionService.GetByIDAsync(id);
            if (d == null || d.Estado != EstadoCaso.Pendiente)
                return RedirectToAction(nameof(Details), new { id });

            d.TipoCaso = model.TipoCaso;
            d.Motivo = model.Motivo;
            d.Descripcion = model.Descripcion;
            d.Comentarios = model.Comentarios;
            d.Usuario = User.Identity?.Name ?? "Sistema";
            d.Items = model.Items
                .Where(i => i.Seleccionado)
                .Select(i => new ItemDevolucionGarantia
                {
                    ItemDevolucionGarantiaID = i.ItemDevolucionGarantiaID,
                    ProductoID = i.ProductoID,
                    NombreProducto = i.NombreProducto,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    ProductoDanado = i.ProductoDanado,
                    EstadoProducto = i.EstadoProducto
                }).ToList();
            d.CambiosProducto = model.CambiosProducto
                .Where(c => c.ProductoNuevoID > 0)
                .Select(c => new CambioProducto
                {
                    CambioProductoID = c.CambioProductoID,
                    ProductoOriginalID = c.ProductoOriginalID,
                    NombreProductoOriginal = c.NombreProductoOriginal,
                    ProductoNuevoID = c.ProductoNuevoID,
                    NombreProductoNuevo = c.NombreProductoNuevo,
                    Cantidad = c.Cantidad,
                    DiferenciaPrecio = c.DiferenciaPrecio
                }).ToList();

            await _devolucionService.UpdateAsync(d);
            TempData["Success"] = $"{model.TipoCaso} actualizada con éxito";
            return RedirectToAction(nameof(Details), new { id });
        }


        // POST: DevolucionGarantia/Procesar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Procesar(int id)
        {
            try
            {
                var devolucion = await _devolucionService.GetByIDAsync(id);
                if (devolucion == null)
                {
                    return NotFound();
                }

                // Solo procesar si está pendiente
                if (devolucion.Estado != EstadoCaso.Pendiente)
                {
                    TempData["Warning"] = "Solo se pueden procesar devoluciones/garantías en estado pendiente";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Procesar según el tipo de caso
                switch (devolucion.TipoCaso)
                {
                    case TipoCaso.Devolucion:
                        await _devolucionService.ProcesarDevolucionAsync(id);
                        TempData["Success"] = "Devolución procesada con éxito";
                        break;
                    case TipoCaso.Cambio:
                        await _devolucionService.ProcesarCambioAsync(id);
                        TempData["Success"] = "Cambio procesado con éxito";
                        break;
                    case TipoCaso.Garantia:
                        // Redirigir a la pantalla de envío a garantía
                        return RedirectToAction(nameof(EnviarGarantia), new { id });
                    case TipoCaso.Reparacion:
                        // Código para procesar reparación
                        break;
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar devolución/garantía");
                TempData["Error"] = "Error al procesar: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: DevolucionGarantia/EnviarGarantia/5
        public async Task<IActionResult> EnviarGarantia(int id)
        {
            try
            {
                var devolucion = await _devolucionService.GetByIDAsync(id);
                if (devolucion == null)
                {
                    return NotFound();
                }

                // Solo enviar si es garantía y está pendiente
                if (devolucion.TipoCaso != TipoCaso.Garantia || devolucion.Estado != EstadoCaso.Pendiente)
                {
                    TempData["Warning"] = "Solo se pueden enviar a garantía los casos de tipo garantía en estado pendiente";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var model = new EnviarGarantiaViewModel
                {
                    DevolucionGarantiaID = devolucion.DevolucionGarantiaID,
                    Destinatario = "", // Sugerir el proveedor si está disponible
                    TrackingNumber = ""
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar envío a garantía");
                return View("Error");
            }
        }

        // POST: DevolucionGarantia/EnviarGarantia/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarGarantia(int id, EnviarGarantiaViewModel model)
        {
            if (id != model.DevolucionGarantiaID)
            {
                return NotFound();
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    return View(model);
                }

                await _devolucionService.EnviarGarantiaAsync(id, model.Destinatario, model.TrackingNumber);

                TempData["Success"] = "Garantía enviada con éxito";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar a garantía");
                ModelState.AddModelError("", "Error al enviar: " + ex.Message);
                return View(model);
            }
        }

        // GET: DevolucionGarantia/CompletarGarantia/5
        public async Task<IActionResult> CompletarGarantia(int id)
        {
            try
            {
                var devolucion = await _devolucionService.GetByIDAsync(id);
                if (devolucion == null)
                {
                    return NotFound();
                }

                // Solo completar si es garantía y está en proceso
                if (devolucion.TipoCaso != TipoCaso.Garantia || devolucion.Estado != EstadoCaso.EnProceso)
                {
                    TempData["Warning"] = "Solo se pueden completar garantías en estado 'En Proceso'";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var model = new CompletarGarantiaViewModel
                {
                    DevolucionGarantiaID = devolucion.DevolucionGarantiaID,
                    Exitoso = true,
                    Resultado = ""
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar completar garantía");
                return View("Error");
            }
        }

        // POST: DevolucionGarantia/CompletarGarantia/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarGarantia(int id, CompletarGarantiaViewModel model)
        {
            if (id != model.DevolucionGarantiaID)
            {
                return NotFound();
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    return View(model);
                }

                await _devolucionService.CompletarGarantiaAsync(id, model.Exitoso, model.Resultado);

                TempData["Success"] = "Garantía completada con éxito";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar garantía");
                ModelState.AddModelError("", "Error al completar: " + ex.Message);
                return View(model);
            }
        }

        // GET: DevolucionGarantia/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var devolucion = await _devolucionService.GetByIDAsync(id);
                if (devolucion == null)
                {
                    return NotFound();
                }

                // Solo permitir eliminar si está pendiente
                if (devolucion.Estado != EstadoCaso.Pendiente)
                {
                    TempData["Warning"] = "Solo se pueden eliminar devoluciones/garantías en estado pendiente";
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(devolucion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al mostrar confirmación de eliminación");
                return View("Error");
            }
        }

        // POST: DevolucionGarantia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var devolucion = await _devolucionService.GetByIDAsync(id);
                if (devolucion == null)
                {
                    return NotFound();
                }

                // Solo permitir eliminar si está pendiente
                if (devolucion.Estado != EstadoCaso.Pendiente)
                {
                    TempData["Warning"] = "Solo se pueden eliminar devoluciones/garantías en estado pendiente";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _devolucionService.DeleteAsync(id);

                TempData["Success"] = "Registro eliminado con éxito";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar devolución/garantía");
                TempData["Error"] = "Error al eliminar: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Métodos auxiliares
        private List<SelectListItem> GetTiposCasoSelectList()
        {
            return Enum.GetValues(typeof(TipoCaso))
                .Cast<TipoCaso>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = t.ToString()
                })
                .ToList();
        }

        private List<SelectListItem> GetMotivosSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Producto defectuoso", Text = "Producto defectuoso" },
                new SelectListItem { Value = "Error en pedido", Text = "Error en pedido" },
                new SelectListItem { Value = "Insatisfacción del cliente", Text = "Insatisfacción del cliente" },
                new SelectListItem { Value = "Garantía del fabricante", Text = "Garantía del fabricante" },
                new SelectListItem { Value = "Cambio de opinión", Text = "Cambio de opinión" },
                new SelectListItem { Value = "Otro", Text = "Otro" }
            };
        }

        private List<SelectListItem> GetEstadosProductoSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Funcional", Text = "Funcional" },
                new SelectListItem { Value = "Defectuoso", Text = "Defectuoso" },
                new SelectListItem { Value = "Dañado", Text = "Dañado" },
                new SelectListItem { Value = "Incompleto", Text = "Incompleto" },
                new SelectListItem { Value = "Sin usar", Text = "Sin usar" }
            };
        }

        private async Task<List<SelectListItem>> GetProductosSelectList()
        {
            var productos = await _productoService.GetAllProductosAsync();
            return productos.Select(p => new SelectListItem
            {
                Value = p.ProductoID.ToString(),
                Text = $"{p.Nombre} ({p.PCosto:C})"
            }).ToList();
        }

        // API para buscar productos para cambio
        [HttpGet]
        public async Task<IActionResult> BuscarProductos(string term)
        {
            try
            {
                var productos = await _productoService.GetProductosByTermAsync(term);
                var result = productos.Select(p => new
                {
                    id = p.ProductoID,
                    label = $"{p.Nombre} ({p.PContado:C})",
                    precio = p.PContado
                }).ToList();
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos");
                return Json(new List<object>());
            }
        }

        // API para obtener precio de producto
        [HttpGet]
        public async Task<IActionResult> GetPrecioProducto(int id)
        {
            try
            {
                var producto = await _productoService.GetProductoByIDAsync(id);
                if (producto == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }
                return Json(new { success = true, precio = producto.PContado });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener precio de producto");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // ViewModel adicional para enviar a garantía
    public class EnviarGarantiaViewModel
    {
        public int DevolucionGarantiaID { get; set; }

        [Required(ErrorMessage = "El destinatario es obligatorio")]
        [Display(Name = "Destinatario")]
        public string Destinatario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de seguimiento es obligatorio")]
        [Display(Name = "Número de Seguimiento")]
        public string TrackingNumber { get; set; } = string.Empty;
    }

    // ViewModel adicional para completar garantía
    public class CompletarGarantiaViewModel
    {
        public int DevolucionGarantiaID { get; set; }

        [Display(Name = "¿Garantía exitosa?")]
        public bool Exitoso { get; set; }

        [Required(ErrorMessage = "El resultado es obligatorio")]
        [Display(Name = "Resultado")]
        public string Resultado { get; set; } = string.Empty;
    }
}
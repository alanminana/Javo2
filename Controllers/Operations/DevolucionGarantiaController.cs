// Controllers/Operations/DevolucionGarantiaController.cs
using Javo2.IServices;
using Javo2.IServices.Common;
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
using Microsoft.AspNetCore.Authorization;

namespace Javo2.Controllers.Operations
{
    public class DevolucionGarantiaController : OperationsBaseController
    {
        private readonly IDevolucionGarantiaService _devolucionService;
        private readonly IVentaService _ventaService;
        private readonly IStockService _stockService;

        public DevolucionGarantiaController(
            IDevolucionGarantiaService devolucionService,
            IVentaService ventaService,
            IProductoService productoService,
            IClienteService clienteService,
            IStockService stockService,
            IAuditoriaService auditoriaService,
            IDropdownService dropdownService,
            IMapper mapper,
            ILogger<DevolucionGarantiaController> logger)
            : base(productoService, clienteService, auditoriaService, dropdownService, mapper, logger)
        {
            _devolucionService = devolucionService;
            _ventaService = ventaService;
            _stockService = stockService;
        }

        // GET: DevolucionGarantia
        [Authorize(Policy = "Permission:devolucionGarantia.ver")]
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
                    CantidadProductos = CalcularCantidadTotalProductos(d.Items, i => i.Cantidad),
                    FechaResolucion = d.FechaResolucion
                }).OrderByDescending(d => d.FechaSolicitud).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cargar el listado de devoluciones/garantías");
                return View("Error");
            }
        }

        // GET: DevolucionGarantia/Create
        [Authorize(Policy = "Permission:devolucionGarantia.crear")]
        public IActionResult Create()
        {
            var model = new DevolucionGarantiaViewModel
            {
                FechaSolicitud = DateTime.Now,
                TiposCaso = GetTiposCasoSelectList(),
                Motivos = GetMotivosSelectList()
            };
            return View("Form", model);
        }

        // POST: DevolucionGarantia/BuscarVenta
        [HttpPost]
        public async Task<IActionResult> BuscarVenta(string numeroFactura)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(numeroFactura))
                {
                    return JsonError("Ingrese un número de factura válido");
                }

                // Buscar la venta por número de factura
                var ventas = await _ventaService.GetAllVentasAsync();
                var venta = ventas.FirstOrDefault(v => v.NumeroFactura.Contains(numeroFactura));

                if (venta == null)
                {
                    return JsonError("No se encontró la venta con ese número de factura");
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

                return JsonSuccess(null, new
                {
                    ventaID = venta.VentaID,
                    numeroFactura = venta.NumeroFactura,
                    cliente = venta.NombreCliente,
                    fecha = venta.FechaVenta.ToString("dd/MM/yyyy"),
                    items
                });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al buscar venta");
                return JsonError("Error al buscar la venta: " + ex.Message);
            }
        }

        // POST: DevolucionGarantia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:devolucionGarantia.crear")]
        public async Task<IActionResult> Create(DevolucionGarantiaViewModel model)
        {
            model.TiposCaso = GetTiposCasoSelectList();
            model.Motivos = GetMotivosSelectList();

            // Usar método común de validación
            if (!ValidarProductosEnOperacion(model.Items?.Where(i => i.Seleccionado), "devolución/garantía"))
            {
                return View("Form", model);
            }

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

            // Usar método común de auditoría
            await RegistrarAuditoriaOperacionAsync(
                "DevolucionGarantia",
                "Create",
                id,
                $"{model.TipoCaso} creada para venta {venta.NumeroFactura}, Cliente: {venta.NombreCliente}"
            );

            SetSuccessMessage($"{model.TipoCaso} registrada con éxito, ID: {id}");
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: DevolucionGarantia/Procesar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:devolucionGarantia.procesar")]
        public async Task<IActionResult> Procesar(int id)
        {
            return await CambiarEstadoOperacionAsync(
                id,
                EstadoCaso.EnProceso,
                _devolucionService.GetByIDAsync,
                async (devolucion, estado) => {
                    switch (devolucion.TipoCaso)
                    {
                        case TipoCaso.Devolucion:
                            await _devolucionService.ProcesarDevolucionAsync(id);
                            break;
                        case TipoCaso.Cambio:
                            await _devolucionService.ProcesarCambioAsync(id);
                            break;
                        case TipoCaso.Garantia:
                            // Redirigir a la pantalla de envío a garantía
                            return false; // Indicar que no se procesó aquí
                        case TipoCaso.Reparacion:
                            // Código para procesar reparación
                            break;
                    }
                    return true;
                },
                "Devolución/Garantía"
            );
        }

        // Resto de métodos similares...
        // [Details, Edit, Delete, etc. - siguiendo el mismo patrón de usar métodos comunes de la clase base]

        #region Métodos auxiliares

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

        [HttpGet]
        public async Task<IActionResult> BuscarProductos(string term)
        {
            // Usar método común de la clase base
            return await BuscarProductosAsync(term);
        }

     
        #endregion
    }
}
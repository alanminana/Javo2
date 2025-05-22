// Controllers/Reports/ReportesController.cs
using Javo2.Controllers.Base;
using Javo2.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Javo2.Models;

namespace Javo2.Controllers.Reports
{
    [Authorize]
    public class ReportesController : BaseController
    {
        private readonly IVentaService _ventaService;
        private readonly IProductoService _productoService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IClienteService _clienteService;

        public ReportesController(
            IVentaService ventaService,
            IProductoService productoService,
            IAuditoriaService auditoriaService,
            IClienteService clienteService,
            ILogger<ReportesController> logger)
            : base(logger)
        {
            _ventaService = ventaService;
            _productoService = productoService;
            _auditoriaService = auditoriaService;
            _clienteService = clienteService;
        }

        // GET: Reportes/Index
        [HttpGet]
        [Authorize(Policy = "Permission:reportes.ver")]
        public IActionResult Index()
        {
            try
            {
                var model = new ReportesIndexViewModel
                {
                    TiposReporte = GetTiposReporteDisponibles(),
                    FechaDesde = DateTime.Now.AddMonths(-1),
                    FechaHasta = DateTime.Now
                };
                return View(model);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cargar la página principal de reportes");
                return View("Error");
            }
        }

        // GET: Reportes/RankingVentas
        [HttpGet]
        [Authorize(Policy = "Permission:reportes.rankingVentas")]
        public async Task<IActionResult> RankingVentas(DateTime? fechaDesde = null, DateTime? fechaHasta = null, int top = 10)
        {
            try
            {
                LogInfo("Generando ranking de ventas. Desde: {FechaDesde}, Hasta: {FechaHasta}, Top: {Top}",
                    fechaDesde, fechaHasta, top);

                // Establecer fechas por defecto si no se proporcionan
                var desde = fechaDesde ?? DateTime.Now.AddMonths(-1);
                var hasta = fechaHasta ?? DateTime.Now;

                // Validar rango de fechas
                if (desde >= hasta)
                {
                    SetErrorMessage("La fecha de inicio debe ser anterior a la fecha de fin");
                    return RedirectToAction(nameof(Index));
                }

                var ventas = await _ventaService.GetAllVentasAsync();

                // Filtrar por rango de fechas
                ventas = ventas.Where(v => v.FechaVenta >= desde && v.FechaVenta <= hasta);

                var ranking = ventas
                    .GroupBy(v => v.NombreCliente)
                    .Select(g => new RankingVentasViewModel
                    {
                        Cliente = g.Key,
                        CantidadVentas = g.Count(),
                        SumaTotal = g.Sum(x => x.PrecioTotal),
                        PromedioVenta = g.Average(x => x.PrecioTotal)
                    })
                    .OrderByDescending(x => x.SumaTotal)
                    .Take(top)
                    .ToList();

                ViewBag.FechaDesde = desde;
                ViewBag.FechaHasta = hasta;
                ViewBag.Top = top;
                ViewBag.TotalVentas = ventas.Sum(v => v.PrecioTotal);
                ViewBag.CantidadVentas = ventas.Count();

                return View(ranking);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error generando ranking de ventas");
                SetErrorMessage("Error al generar el ranking de ventas");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Reportes/ExportVentasToExcel
        [HttpGet]
        [Authorize(Policy = "Permission:reportes.exportVentas")]
        public async Task<IActionResult> ExportVentasToExcel(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                LogInfo("Exportando ventas a Excel. Desde: {FechaDesde}, Hasta: {FechaHasta}", fechaDesde, fechaHasta);

                var desde = fechaDesde ?? DateTime.Now.AddMonths(-1);
                var hasta = fechaHasta ?? DateTime.Now;

                var ventas = await _ventaService.GetAllVentasAsync();
                ventas = ventas.Where(v => v.FechaVenta >= desde && v.FechaVenta <= hasta);

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Ventas");

                    // Encabezados
                    ws.Cell(1, 1).Value = "Número Factura";
                    ws.Cell(1, 2).Value = "Fecha";
                    ws.Cell(1, 3).Value = "Cliente";
                    ws.Cell(1, 4).Value = "DNI Cliente";
                    ws.Cell(1, 5).Value = "Total";
                    ws.Cell(1, 6).Value = "Estado";
                    ws.Cell(1, 7).Value = "Forma Pago";
                    ws.Cell(1, 8).Value = "Vendedor";
                    ws.Cell(1, 9).Value = "Observaciones";

                    // Estilo para encabezados
                    var headerRange = ws.Range("A1:I1");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

                    // Datos
                    int row = 2;
                    foreach (var v in ventas.OrderByDescending(x => x.FechaVenta))
                    {
                        ws.Cell(row, 1).Value = v.NumeroFactura;
                        ws.Cell(row, 2).Value = v.FechaVenta.ToString("dd/MM/yyyy");
                        ws.Cell(row, 3).Value = v.NombreCliente;
                        ws.Cell(row, 4).Value = v.DniCliente;
                        ws.Cell(row, 5).Value = v.PrecioTotal;
                        ws.Cell(row, 6).Value = v.Estado.ToString();
                        ws.Cell(row, 7).Value = v.FormaPago;
                        ws.Cell(row, 8).Value = v.Vendedor;
                        ws.Cell(row, 9).Value = v.Observaciones ?? "";
                        row++;
                    }

                    // Ajustar columnas y agregar totales
                    ws.Columns().AdjustToContents();

                    // Agregar fila de totales
                    ws.Cell(row + 1, 4).Value = "TOTAL:";
                    ws.Cell(row + 1, 4).Style.Font.Bold = true;
                    ws.Cell(row + 1, 5).Value = ventas.Sum(v => v.PrecioTotal);
                    ws.Cell(row + 1, 5).Style.Font.Bold = true;
                    ws.Cell(row + 1, 5).Style.NumberFormat.Format = "$#,##0.00";

                    using (var ms = new MemoryStream())
                    {
                        workbook.SaveAs(ms);
                        ms.Position = 0;

                        var fileName = $"Ventas_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx";

                        return File(
                            ms.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error exportando ventas a Excel");
                SetErrorMessage("Error al generar el archivo Excel de ventas");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Reportes/ReporteStock
        [HttpGet]
        [Authorize(Policy = "Permission:reportes.reporteStock")]
        public async Task<IActionResult> ReporteStock(string filtroStock = "todos")
        {
            try
            {
                LogInfo("Generando reporte de stock. Filtro: {Filtro}", filtroStock);

                var productos = await _productoService.GetAllProductosAsync();

                var data = productos
                    .Select(p => new ReporteStockViewModel
                    {
                        ProductoID = p.ProductoID,
                        CodigoAlfa = p.CodigoAlfa,
                        CodigoBarra = p.CodigoBarra,
                        Nombre = p.Nombre,
                        Marca = p.Marca?.Nombre ?? "Sin marca",
                        Rubro = p.Rubro?.Nombre ?? "Sin rubro",
                        SubRubro = p.SubRubro?.Nombre ?? "",
                        Stock = p.StockItem?.CantidadDisponible ?? 0,
                        StockMinimo = p.StockMinimo ?? 5, // Default si no está definido
                        PrecioCosto = p.PCosto,
                        PrecioVenta = p.PContado,
                        ValorStock = (p.StockItem?.CantidadDisponible ?? 0) * p.PCosto
                    })
                    .ToList();

                // Aplicar filtros
                switch (filtroStock.ToLower())
                {
                    case "sinstock":
                        data = data.Where(x => x.Stock == 0).ToList();
                        break;
                    case "stockbajo":
                        data = data.Where(x => x.Stock > 0 && x.Stock <= x.StockMinimo).ToList();
                        break;
                    case "stockalto":
                        data = data.Where(x => x.Stock > x.StockMinimo * 3).ToList();
                        break;
                    case "critico":
                        data = data.Where(x => x.Stock <= (x.StockMinimo * 0.5)).ToList();
                        break;
                    default: // "todos"
                        break;
                }

                data = data.OrderByDescending(x => x.ValorStock).ToList();

                ViewBag.FiltroStock = filtroStock;
                ViewBag.TotalProductos = data.Count;
                ViewBag.ValorTotalStock = data.Sum(x => x.ValorStock);
                ViewBag.ProductosSinStock = productos.Count(p => (p.StockItem?.CantidadDisponible ?? 0) == 0);
                ViewBag.ProductosStockBajo = productos.Count(p => {
                    var stock = p.StockItem?.CantidadDisponible ?? 0;
                    var minimo = p.StockMinimo ?? 5;
                    return stock > 0 && stock <= minimo;
                });

                return View(data);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error generando reporte de stock");
                SetErrorMessage("Error al generar el reporte de stock");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Reportes/ReporteClientes
        [HttpGet]
        [Authorize(Policy = "Permission:reportes.reporteClientes")]
        public async Task<IActionResult> ReporteClientes()
        {
            try
            {
                LogInfo("Generando reporte de clientes");

                var clientes = await _clienteService.GetAllClientesAsync();
                var ventas = await _ventaService.GetAllVentasAsync();

                var data = clientes.Select(c => {
                    var ventasCliente = ventas.Where(v => v.DniCliente == c.DNI).ToList();
                    return new ReporteClientesViewModel
                    {
                        ClienteID = c.ClienteID,
                        Nombre = $"{c.Nombre} {c.Apellido}",
                        DNI = c.DNI,
                        Email = c.Email,
                        Telefono = c.Telefono,
                        Celular = c.Celular,
                        Localidad = c.Localidad,
                        FechaRegistro = c.FechaCreacion,
                        AptoCredito = c.AptoCredito,
                        LimiteCredito = c.LimiteCreditoInicial,
                        SaldoActual = c.Saldo,
                        CantidadCompras = ventasCliente.Count,
                        TotalComprado = ventasCliente.Sum(v => v.PrecioTotal),
                        UltimaCompra = ventasCliente.OrderByDescending(v => v.FechaVenta)
                                                  .FirstOrDefault()?.FechaVenta,
                        PromedioCompra = ventasCliente.Any() ? ventasCliente.Average(v => v.PrecioTotal) : 0
                    };
                }).OrderByDescending(x => x.TotalComprado).ToList();

                ViewBag.TotalClientes = data.Count;
                ViewBag.ClientesActivos = data.Count(c => c.CantidadCompras > 0);
                ViewBag.ClientesCredito = data.Count(c => c.AptoCredito);
                ViewBag.TotalVentas = data.Sum(c => c.TotalComprado);

                return View(data);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error generando reporte de clientes");
                SetErrorMessage("Error al generar el reporte de clientes");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Reportes/ReporteAuditoria
        [HttpGet]
        [Authorize(Policy = "Permission:reportes.auditoria")]
        public async Task<IActionResult> ReporteAuditoria(DateTime? fechaDesde = null, DateTime? fechaHasta = null,
            string entidad = null, string accion = null)
        {
            try
            {
                LogInfo("Generando reporte de auditoría");

                var desde = fechaDesde ?? DateTime.Now.AddDays(-7);
                var hasta = fechaHasta ?? DateTime.Now;

                var registros = await _auditoriaService.GetAllRegistrosAsync();

                // Filtrar por fecha
                registros = registros.Where(r => r.FechaHora >= desde && r.FechaHora <= hasta);

                // Filtrar por entidad si se especifica
                if (!string.IsNullOrEmpty(entidad))
                {
                    registros = registros.Where(r => r.Entidad.Equals(entidad, StringComparison.OrdinalIgnoreCase));
                }

                // Filtrar por acción si se especifica
                if (!string.IsNullOrEmpty(accion))
                {
                    registros = registros.Where(r => r.Accion.Equals(accion, StringComparison.OrdinalIgnoreCase));
                }

                var data = registros.OrderByDescending(r => r.FechaHora).ToList();

                ViewBag.FechaDesde = desde;
                ViewBag.FechaHasta = hasta;
                ViewBag.EntidadFiltro = entidad;
                ViewBag.AccionFiltro = accion;
                ViewBag.TotalRegistros = data.Count;

                // Para dropdowns de filtros
                ViewBag.Entidades = registros.Select(r => r.Entidad).Distinct().OrderBy(e => e)
                    .Select(e => new SelectListItem { Value = e, Text = e }).ToList();
                ViewBag.Acciones = registros.Select(r => r.Accion).Distinct().OrderBy(a => a)
                    .Select(a => new SelectListItem { Value = a, Text = a }).ToList();

                return View(data);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error generando reporte de auditoría");
                SetErrorMessage("Error al generar el reporte de auditoría");
                return RedirectToAction(nameof(Index));
            }
        }

        #region Métodos Auxiliares

        private List<SelectListItem> GetTiposReporteDisponibles()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Seleccione un tipo de reporte" },
                new SelectListItem { Value = "ventas", Text = "Reporte de Ventas" },
                new SelectListItem { Value = "stock", Text = "Reporte de Stock" },
                new SelectListItem { Value = "clientes", Text = "Reporte de Clientes" },
                new SelectListItem { Value = "auditoria", Text = "Reporte de Auditoría" },
                new SelectListItem { Value = "ranking", Text = "Ranking de Ventas" }
            };
        }

        #endregion
    }

    #region ViewModels para Reportes

    public class ReportesIndexViewModel
    {
        public List<SelectListItem> TiposReporte { get; set; } = new();
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public string TipoReporte { get; set; }
    }

    public class RankingVentasViewModel
    {
        public string Cliente { get; set; }
        public int CantidadVentas { get; set; }
        public decimal SumaTotal { get; set; }
        public decimal PromedioVenta { get; set; }
    }

    public class ReporteStockViewModel
    {
        public int ProductoID { get; set; }
        public string CodigoAlfa { get; set; }
        public string CodigoBarra { get; set; }
        public string Nombre { get; set; }
        public string Marca { get; set; }
        public string Rubro { get; set; }
        public string SubRubro { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal ValorStock { get; set; }
    }

    public class ReporteClientesViewModel
    {
        public int ClienteID { get; set; }
        public string Nombre { get; set; }
        public int DNI { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Celular { get; set; }
        public string Localidad { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool AptoCredito { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal SaldoActual { get; set; }
        public int CantidadCompras { get; set; }
        public decimal TotalComprado { get; set; }
        public DateTime? UltimaCompra { get; set; }
        public decimal PromedioCompra { get; set; }
    }

    #endregion
}
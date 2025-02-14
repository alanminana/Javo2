// File: Controllers/ReportesController.cs
using Javo2.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Javo2.Controllers
{
    public class ReportesController : Controller
    {
        private readonly IVentaService _ventaService;
        private readonly IProductoService _productoService;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(
            IVentaService ventaService,
            IProductoService productoService,
            ILogger<ReportesController> logger)
        {
            _ventaService = ventaService;
            _productoService = productoService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // EJEMPLO 1: Ranking de Ventas
        public async Task<IActionResult> RankingVentas()
        {
            var ventas = await _ventaService.GetAllVentasAsync();
            var ranking = ventas
                .GroupBy(v => v.NombreCliente)
                .Select(g => new {
                    Cliente = g.Key,
                    SumaTotal = g.Sum(x => x.PrecioTotal)
                })
                .OrderByDescending(x => x.SumaTotal)
                .Take(10)
                .ToList();

            return View(ranking);
        }

        // EJEMPLO 2: Exportar Ventas a Excel
        public async Task<IActionResult> ExportVentasToExcel()
        {
            var ventas = await _ventaService.GetAllVentasAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Ventas");
                ws.Cell(1, 1).Value = "VentaID";
                ws.Cell(1, 2).Value = "Fecha";
                ws.Cell(1, 3).Value = "Cliente";
                ws.Cell(1, 4).Value = "Total";
                ws.Cell(1, 5).Value = "Estado";

                int row = 2;
                foreach (var v in ventas)
                {
                    ws.Cell(row, 1).Value = v.VentaID;
                    ws.Cell(row, 2).Value = v.FechaVenta.ToString("dd/MM/yyyy");
                    ws.Cell(row, 3).Value = v.NombreCliente;
                    ws.Cell(row, 4).Value = v.PrecioTotal;
                    ws.Cell(row, 5).Value = v.Estado.ToString();
                    row++;
                }

                using (var ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    ms.Position = 0;
                    return File(
                        ms.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Ventas.xlsx"
                    );
                }
            }
        }

        // EJEMPLO 3: Reporte de Stock
        public async Task<IActionResult> ReporteStock()
        {
            var productos = await _productoService.GetAllProductosAsync();
            var data = productos
                .Select(p => new {
                    p.ProductoID,
                    p.Nombre,
                    Stock = p.StockItem?.CantidadDisponible ?? 0
                })
                .OrderByDescending(x => x.Stock)
                .ToList();

            return View(data);
        }
    }
}

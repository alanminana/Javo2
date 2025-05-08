// Controllers/ProveedoresController.cs (parcial, nuevos métodos)
using Microsoft.AspNetCore.Mvc;
using Javo2.IServices;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Proveedores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.Spreadsheet;
using Javo2.Controllers.Base;
using Javo2.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Javo2.Controllers
{
    [Authorize]
    public class ProveedoresController : BaseController
    {
        // Propiedades y constructor existentes

        // Métodos para gestión de compras
        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> Compras()
        {
            try
            {
                var compras = await _proveedorService.GetComprasAsync();
                var model = _mapper.Map<IEnumerable<CompraProveedorViewModel>>(compras);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar compras");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> DetallesCompra(int id)
        {
            try
            {
                var compra = await _proveedorService.GetCompraByIDAsync(id);
                if (compra == null) return NotFound();

                var model = _mapper.Map<CompraProveedorViewModel>(compra);

                // Cargar nombre del proveedor
                var proveedor = await _proveedorService.GetProveedorByIDAsync(compra.ProveedorID);
                if (proveedor != null)
                {
                    model.NombreProveedor = proveedor.Nombre;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles de compra");
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.crear")]
        public async Task<IActionResult> CrearCompra(int? proveedorId)
        {
            try
            {
                var model = new CompraProveedorViewModel
                {
                    FechaCompra = DateTime.Now,
                    NumeroFactura = await _proveedorService.GenerarNumeroFacturaCompraAsync(),
                    Usuario = User.Identity?.Name ?? "Desconocido"
                };

                if (proveedorId.HasValue)
                {
                    var proveedor = await _proveedorService.GetProveedorByIDAsync(proveedorId.Value);
                    if (proveedor != null)
                    {
                        model.ProveedorID = proveedor.ProveedorID;
                        model.NombreProveedor = proveedor.Nombre;
                    }
                }

                // Cargar listas de opciones
                await CargarOpcionesParaCompraAsync(model);

                return View("FormCompra", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar formulario de compra");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.crear")]
        public async Task<IActionResult> CrearCompra(CompraProveedorViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    await CargarOpcionesParaCompraAsync(model);
                    return View("FormCompra", model);
                }

                // Validar que haya productos
                if (model.ProductosCompra == null || !model.ProductosCompra.Any())
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto a la compra");
                    await CargarOpcionesParaCompraAsync(model);
                    return View("FormCompra", model);
                }

                // Convertir ViewModel a modelo
                var compra = _mapper.Map<CompraProveedor>(model);
                compra.TotalProductos = compra.ProductosCompra.Sum(p => p.Cantidad);
                compra.PrecioTotal = compra.ProductosCompra.Sum(p => p.PrecioTotal);
                compra.Usuario = User.Identity?.Name ?? "Desconocido";
                compra.Estado = Enum.Parse<EstadoCompra>(model.Estado);

                // Crear compra
                await _proveedorService.CreateCompraAsync(compra);

                TempData["Success"] = "Compra creada exitosamente.";
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear compra");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear la compra: " + ex.Message);
                await CargarOpcionesParaCompraAsync(model);
                return View("FormCompra", model);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> EditarCompra(int id)
        {
            try
            {
                var compra = await _proveedorService.GetCompraByIDAsync(id);
                if (compra == null) return NotFound();

                var model = _mapper.Map<CompraProveedorViewModel>(compra);

                // Verificar si la compra está en estado Completada o Cancelada
                if (compra.Estado == EstadoCompra.Completada || compra.Estado == EstadoCompra.Cancelada)
                {
                    TempData["Error"] = "No se puede editar una compra completada o cancelada.";
                    return RedirectToAction(nameof(DetallesCompra), new { id });
                }

                // Cargar nombre del proveedor
                var proveedor = await _proveedorService.GetProveedorByIDAsync(compra.ProveedorID);
                if (proveedor != null)
                {
                    model.NombreProveedor = proveedor.Nombre;
                }

                await CargarOpcionesParaCompraAsync(model);

                return View("FormCompra", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar compra para edición");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> EditarCompra(CompraProveedorViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LogModelStateErrors();
                    await CargarOpcionesParaCompraAsync(model);
                    return View("FormCompra", model);
                }

                // Validar que haya productos
                if (model.ProductosCompra == null || !model.ProductosCompra.Any())
                {
                    ModelState.AddModelError("", "Debe agregar al menos un producto a la compra");
                    await CargarOpcionesParaCompraAsync(model);
                    return View("FormCompra", model);
                }

                // Verificar estado de la compra
                var compraOriginal = await _proveedorService.GetCompraByIDAsync(model.CompraID);
                if (compraOriginal == null) return NotFound();

                if (compraOriginal.Estado == EstadoCompra.Completada || compraOriginal.Estado == EstadoCompra.Cancelada)
                {
                    TempData["Error"] = "No se puede editar una compra completada o cancelada.";
                    return RedirectToAction(nameof(DetallesCompra), new { id = model.CompraID });
                }

                // Convertir ViewModel a modelo
                var compra = _mapper.Map<CompraProveedor>(model);
                compra.TotalProductos = compra.ProductosCompra.Sum(p => p.Cantidad);
                compra.PrecioTotal = compra.ProductosCompra.Sum(p => p.PrecioTotal);
                compra.Usuario = User.Identity?.Name ?? "Desconocido";
                compra.Estado = Enum.Parse<EstadoCompra>(model.Estado);

                // Actualizar compra
                await _proveedorService.UpdateCompraAsync(compra);

                TempData["Success"] = "Compra actualizada exitosamente.";
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar compra");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar la compra: " + ex.Message);
                await CargarOpcionesParaCompraAsync(model);
                return View("FormCompra", model);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:proveedores.eliminar")]
        public async Task<IActionResult> EliminarCompra(int id)
        {
            try
            {
                var compra = await _proveedorService.GetCompraByIDAsync(id);
                if (compra == null) return NotFound();

                var model = _mapper.Map<CompraProveedorViewModel>(compra);

                // Cargar nombre del proveedor
                var proveedor = await _proveedorService.GetProveedorByIDAsync(compra.ProveedorID);
                if (proveedor != null)
                {
                    model.NombreProveedor = proveedor.Nombre;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar compra para eliminación");
                return View("Error");
            }
        }

        [HttpPost, ActionName("EliminarCompra")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.eliminar")]
        public async Task<IActionResult> EliminarCompraConfirmado(int id)
        {
            try
            {
                await _proveedorService.DeleteCompraAsync(id);
                TempData["Success"] = "Compra eliminada exitosamente.";
                return RedirectToAction(nameof(Compras));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar compra");
                TempData["Error"] = "Ocurrió un error al eliminar la compra: " + ex.Message;
                return RedirectToAction(nameof(Compras));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> ProcesarCompra(int id)
        {
            try
            {
                await _proveedorService.ProcesarCompraAsync(id);
                TempData["Success"] = "Compra procesada exitosamente.";
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar compra");
                TempData["Error"] = "Ocurrió un error al procesar la compra: " + ex.Message;
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> CompletarCompra(int id)
        {
            try
            {
                await _proveedorService.CompletarCompraAsync(id);
                TempData["Success"] = "Compra completada exitosamente.";
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar compra");
                TempData["Error"] = "Ocurrió un error al completar la compra: " + ex.Message;
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permission:proveedores.editar")]
        public async Task<IActionResult> CancelarCompra(int id)
        {
            try
            {
                await _proveedorService.CancelarCompraAsync(id);
                TempData["Success"] = "Compra cancelada exitosamente.";
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar compra");
                TempData["Error"] = "Ocurrió un error al cancelar la compra: " + ex.Message;
                return RedirectToAction(nameof(DetallesCompra), new { id });
            }
        }

        [HttpPost]
        [Authorize(Policy = "Permission:proveedores.ver")]
        public async Task<IActionResult> BuscarProducto(string codigoProducto)
        {
            try
            {
                var producto = await _productoService.GetProductoByCodigoAsync(codigoProducto);
                if (producto == null)
                    return Json(new { success = false, message = "Producto no encontrado." });

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        productoID = producto.ProductoID,
                        codigoBarra = producto.CodigoBarra,
                        codigoAlfa = producto.CodigoAlfa,
                        nombreProducto = producto.Nombre,
                        marca = producto.Marca?.Nombre ?? "",
                        cantidad = 1,
                        precioUnitario = producto.PCosto,
                        precioTotal = producto.PCosto
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar producto");
                return Json(new { success = false, message = "Error al buscar el producto." });
            }
        }

        // Método auxiliar para cargar opciones de compra
        private async Task CargarOpcionesParaCompraAsync(CompraProveedorViewModel model)
        {
            // Cargar listas de opciones para los desplegables
            var ventaService = HttpContext.RequestServices.GetService(typeof(IVentaService)) as IVentaService;
            if (ventaService != null)
            {
                model.FormasPago = ventaService.GetFormasPagoSelectList();
                model.Bancos = ventaService.GetBancosSelectList();
                model.TipoTarjetaOptions = ventaService.GetTipoTarjetaSelectList();
                model.CuotasOptions = ventaService.GetCuotasSelectList();
                model.EntidadesElectronicas = ventaService.GetEntidadesElectronicasSelectList();
            }

            // Cargar lista de proveedores
            var proveedores = await _proveedorService.GetProveedoresAsync();
            model.Proveedores = proveedores.Select(p => new SelectListItem
            {
                Value = p.ProveedorID.ToString(),
                Text = p.Nombre
            });
        }

        // Aquí irían el resto de los métodos que ya tienes en el controlador
    }
}
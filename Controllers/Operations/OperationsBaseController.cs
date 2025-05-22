// Controllers/Operations/OperationsBaseController.cs
using AutoMapper;
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.IServices.Common;
using Javo2.Models;
using Javo2.ViewModels.Operaciones.Ventas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Operations
{
    public abstract class OperationsBaseController : BaseController
    {
        protected readonly IProductoService _productoService;
        protected readonly IClienteService _clienteService;
        protected readonly IAuditoriaService _auditoriaService;
        protected readonly IDropdownService _dropdownService;
        protected readonly IMapper _mapper;

        public OperationsBaseController(
            IProductoService productoService,
            IClienteService clienteService,
            IAuditoriaService auditoriaService,
            IDropdownService dropdownService,
            IMapper mapper,
            ILogger logger) : base(logger)
        {
            _productoService = productoService;
            _clienteService = clienteService;
            _auditoriaService = auditoriaService;
            _dropdownService = dropdownService;
            _mapper = mapper;
        }

        #region Búsqueda común de productos

        protected async Task<IActionResult> BuscarProductoPorCodigoAsync(string codigoProducto)
        {
            try
            {
                LogInfo("Buscando producto por código: {Codigo}", codigoProducto);

                if (string.IsNullOrWhiteSpace(codigoProducto))
                {
                    return JsonError("Debe proporcionar un código de producto válido");
                }

                var producto = await _productoService.GetProductoByCodigoAsync(codigoProducto);

                if (producto == null)
                {
                    // Intentar buscar por término si no se encuentra por código exacto
                    var productos = await _productoService.GetProductosByTermAsync(codigoProducto);
                    producto = productos.FirstOrDefault();

                    if (producto == null)
                        return JsonError("Producto no encontrado.");
                }

                return JsonSuccess(null, new
                {
                    productoID = producto.ProductoID,
                    codigoBarra = producto.CodigoBarra,
                    codigoAlfa = producto.CodigoAlfa,
                    nombreProducto = producto.Nombre,
                    marca = producto.Marca?.Nombre ?? "",
                    cantidad = 1,
                    precioUnitario = Math.Round(producto.PContado, 2),
                    precioLista = Math.Round(producto.PLista, 2),
                    pCosto = Math.Round(producto.PCosto, 2),
                    descripcion = producto.Descripcion
                });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al buscar producto por código: {Codigo}", codigoProducto);
                return JsonError("Error al buscar el producto.");
            }
        }

        protected async Task<IActionResult> BuscarProductosAsync(string term, bool forPurchase = false)
        {
            try
            {
                if (string.IsNullOrEmpty(term) || term.Length < 2)
                    return Json(new List<object>());

                var productos = await _productoService.GetAllProductosAsync();
                var query = productos.Where(p =>
                    p.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    p.CodigoAlfa.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    p.CodigoBarra.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    p.Marca != null && p.Marca.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase));

                if (forPurchase)
                {
                    return Json(query.Take(20).Select(p => new {
                        id = p.ProductoID,
                        name = p.Nombre,
                        codigo = p.CodigoAlfa,
                        marca = p.Marca?.Nombre ?? "Sin marca",
                        precio = p.PCosto
                    }));
                }
                else
                {
                    return Json(query.Take(10).Select(p => new {
                        label = p.Nombre,
                        value = p.ProductoID,
                        marca = p.Marca?.Nombre ?? "Sin marca",
                        precio = p.PContado
                    }));
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al buscar productos");
                return Json(new List<object>());
            }
        }

        #endregion

        #region Búsqueda común de clientes

        protected async Task<IActionResult> BuscarClientePorDNIAsync(int dni)
        {
            try
            {
                LogInfo("Buscando cliente por DNI: {DNI}", dni);

                var cliente = await _clienteService.GetClienteByDNIAsync(dni);
                if (cliente == null)
                {
                    return JsonError("Cliente no encontrado con ese DNI.");
                }

                // Determinar si el cliente puede usar crédito
                decimal saldoDisponible = cliente.AptoCredito ? cliente.SaldoDisponible : 0;

                return JsonSuccess(null, new
                {
                    clienteID = cliente.ClienteID,
                    dni = cliente.DNI,
                    nombre = $"{cliente.Nombre} {cliente.Apellido}",
                    telefono = cliente.Telefono,
                    domicilio = $"{cliente.Calle} {cliente.NumeroCalle}",
                    localidad = cliente.Localidad,
                    celular = cliente.Celular,
                    limiteCredito = cliente.LimiteCreditoInicial,
                    saldo = cliente.Saldo,
                    saldoDisponible,
                    aptoCredito = cliente.AptoCredito,
                    email = cliente.Email
                });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al buscar cliente por DNI: {DNI}", dni);
                return JsonError("Error al buscar el cliente.");
            }
        }

        #endregion

        #region Métodos comunes de auditoría para operaciones

        protected async Task RegistrarAuditoriaOperacionAsync(string entidad, string accion, int id, string detalle, string usuario = null)
        {
            try
            {
                usuario ??= User.Identity?.Name ?? "Sistema";

                await _auditoriaService.RegistrarCambioAsync(new AuditoriaRegistro
                {
                    FechaHora = DateTime.Now,
                    Usuario = usuario,
                    Entidad = entidad,
                    Accion = accion,
                    LlavePrimaria = id.ToString(),
                    Detalle = detalle
                });

                LogInfo("Auditoría registrada: {Entidad} {Accion} ID:{ID} por {Usuario}", entidad, accion, id, usuario);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al registrar auditoría para {Entidad} {Accion} ID:{ID}", entidad, accion, id);
            }
        }

        #endregion

        #region Métodos comunes para dropdowns y opciones

        protected async Task CargarOpcionesFormasPagoAsync<T>(T model) where T : class
        {
            var formasPagoProperty = typeof(T).GetProperty("FormasPago");
            var bancosProperty = typeof(T).GetProperty("Bancos");
            var tipoTarjetaProperty = typeof(T).GetProperty("TipoTarjetaOptions");
            var cuotasProperty = typeof(T).GetProperty("CuotasOptions");
            var entidadesElectronicasProperty = typeof(T).GetProperty("EntidadesElectronicas");

            formasPagoProperty?.SetValue(model, _dropdownService.GetFormasPagoList());
            bancosProperty?.SetValue(model, _dropdownService.GetBancosList());
            tipoTarjetaProperty?.SetValue(model, _dropdownService.GetTiposTarjetaList());
            cuotasProperty?.SetValue(model, _dropdownService.GetCuotasList());
            entidadesElectronicasProperty?.SetValue(model, new List<SelectListItem>
            {
                new SelectListItem { Value = "MercadoPago", Text = "MercadoPago" },
                new SelectListItem { Value = "Todo Pago", Text = "Todo Pago" },
                new SelectListItem { Value = "PayPal", Text = "PayPal" }
            });
        }

        protected async Task CargarProveedoresAsync<T>(T model) where T : class
        {
            try
            {
                var proveedoresProperty = typeof(T).GetProperty("Proveedores");
                if (proveedoresProperty != null)
                {
                    // Necesitaremos un servicio de proveedores aquí
                    // Por ahora, dejamos la implementación básica
                    var proveedores = new List<SelectListItem>();
                    proveedoresProperty.SetValue(model, proveedores);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cargar proveedores");
            }
        }

        #endregion

        #region Validaciones comunes

        protected bool ValidarProductosEnOperacion<TDetalle>(IEnumerable<TDetalle> productos, string nombreOperacion)
            where TDetalle : class
        {
            if (productos == null || !productos.Any())
            {
                ModelState.AddModelError("", $"Debe agregar al menos un producto a la {nombreOperacion.ToLower()}");
                return false;
            }

            return true;
        }

        protected bool ValidarEstadoParaEdicion<TEstado>(TEstado estadoActual, TEstado[] estadosPermitidos, string nombreOperacion)
            where TEstado : Enum
        {
            if (!estadosPermitidos.Contains(estadoActual))
            {
                var estadosTexto = string.Join(", ", estadosPermitidos.Select(e => e.ToString()));
                SetErrorMessage($"Solo se pueden editar {nombreOperacion.ToLower()}s en estado: {estadosTexto}");
                return false;
            }

            return true;
        }

        protected bool ValidarEstadoParaEliminacion<TEstado>(TEstado estadoActual, TEstado[] estadosPermitidos, string nombreOperacion)
            where TEstado : Enum
        {
            if (!estadosPermitidos.Contains(estadoActual))
            {
                var estadosTexto = string.Join(", ", estadosPermitidos.Select(e => e.ToString()));
                SetErrorMessage($"Solo se pueden eliminar {nombreOperacion.ToLower()}s en estado: {estadosTexto}");
                return false;
            }

            return true;
        }

        #endregion

        #region Manejo común de estados

        protected async Task<IActionResult> CambiarEstadoOperacionAsync<TEntidad, TEstado>(
            int id,
            TEstado nuevoEstado,
            Func<int, Task<TEntidad>> obtenerEntidad,
            Func<TEntidad, TEstado, Task<bool>> cambiarEstado,
            string nombreEntidad)
            where TEntidad : class
            where TEstado : Enum
        {
            try
            {
                var entidad = await obtenerEntidad(id);
                if (entidad == null)
                {
                    return JsonError($"{nombreEntidad} no encontrada");
                }

                var resultado = await cambiarEstado(entidad, nuevoEstado);
                if (!resultado)
                {
                    return JsonError($"No se pudo cambiar el estado de la {nombreEntidad.ToLower()}");
                }

                return JsonSuccess($"Estado de {nombreEntidad.ToLower()} cambiado correctamente a {nuevoEstado}");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al cambiar estado de {Entidad} ID:{ID} a {Estado}", nombreEntidad, id, nuevoEstado);
                return JsonError($"Error al cambiar estado de la {nombreEntidad.ToLower()}");
            }
        }

        #endregion

        #region Métodos auxiliares para filtrado

        protected async Task<IActionResult> FiltrarEntidadesAsync<TEntidad, TViewModel>(
            Func<Task<IEnumerable<TEntidad>>> obtenerEntidades,
            Func<IEnumerable<TEntidad>, string, string, IEnumerable<TEntidad>> aplicarFiltro,
            string filterField,
            string filterValue,
            string partialViewName)
            where TEntidad : class
            where TViewModel : class
        {
            try
            {
                var entidades = await obtenerEntidades();
                var entidadesFiltradas = aplicarFiltro(entidades, filterField, filterValue);
                var viewModels = _mapper.Map<IEnumerable<TViewModel>>(entidadesFiltradas);

                return PartialView(partialViewName, viewModels);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error al filtrar entidades");
                return PartialView(partialViewName, new List<TViewModel>());
            }
        }

        #endregion

        #region Métodos de cálculo comunes

        protected decimal CalcularTotalOperacion<TDetalle>(IEnumerable<TDetalle> detalles, Func<TDetalle, decimal> obtenerTotal)
        {
            if (detalles == null || !detalles.Any())
                return 0;

            return detalles.Sum(obtenerTotal);
        }

        protected int CalcularCantidadTotalProductos<TDetalle>(IEnumerable<TDetalle> detalles, Func<TDetalle, int> obtenerCantidad)
        {
            if (detalles == null || !detalles.Any())
                return 0;

            return detalles.Sum(obtenerCantidad);
        }

        #endregion

        #region Métodos de generación de códigos

        protected string GenerarNumeroOperacion(string prefijo, int numero)
        {
            return $"{prefijo}-{DateTime.Now:yyyy}-{numero:D6}";
        }

        protected string GenerarNumeroOperacionConFecha(string prefijo, DateTime fecha, int numero)
        {
            return $"{prefijo}-{fecha:yyyyMMdd}-{numero:D4}";
        }

        #endregion
    }
}
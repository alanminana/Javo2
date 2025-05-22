// Controllers/Base/ValidationBaseController.cs
using Javo2.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Javo2.Controllers.Base
{
    public abstract class ValidationBaseController : BaseController
    {
        protected ValidationBaseController(ILogger logger) : base(logger) { }

        #region Validaciones Comunes - Se repiten en TODOS los controllers

        /// <summary>
        /// Valida entidad usando Data Annotations - REPETIDO en ProductosController, ClientesController, etc.
        /// </summary>
        protected bool ValidateEntity<T>(T entity, string prefix = "") where T : class
        {
            if (entity == null)
            {
                ModelState.AddModelError($"{prefix}Entity", "La entidad es requerida");
                return false;
            }

            var context = new ValidationContext(entity);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(entity, context, results, true);

            foreach (var result in results)
            {
                foreach (var memberName in result.MemberNames)
                {
                    ModelState.AddModelError($"{prefix}{memberName}", result.ErrorMessage);
                }
            }

            return isValid && ModelState.IsValid;
        }

        /// <summary>
        /// Verifica códigos únicos - REPETIDO en ProductosController (CodigoAlfa), ClientesController (DNI), etc.
        /// </summary>
        protected void AddUniqueValidationError(string field, string value, string entityType)
        {
            ModelState.AddModelError(field, $"Ya existe {entityType.ToLower()} con {field.ToLower()}: {value}");
        }

        /// <summary>
        /// Valida rangos de fechas - REPETIDO en VentasController, DevolucionGarantiaController, etc.
        /// </summary>
        protected bool ValidateDateRange(DateTime? startDate, DateTime? endDate, string startField = "FechaInicio", string endField = "FechaFin")
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
            {
                ModelState.AddModelError(endField, "La fecha de fin debe ser posterior a la fecha de inicio");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Valida que al menos un elemento esté seleccionado - REPETIDO en VentasController, ProveedoresController, etc.
        /// </summary>
        protected bool ValidateAtLeastOneSelected<T>(IEnumerable<T> items, Func<T, bool> selector, string errorMessage)
        {
            if (items == null || !items.Any(selector))
            {
                ModelState.AddModelError("", errorMessage);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Valida campos monetarios - REPETIDO en VentasController, ProductosController, etc.
        /// </summary>
        protected bool ValidateMonetaryValue(decimal value, string fieldName, decimal? minValue = null, decimal? maxValue = null)
        {
            if (value < 0)
            {
                ModelState.AddModelError(fieldName, $"{fieldName} no puede ser negativo");
                return false;
            }

            if (minValue.HasValue && value < minValue.Value)
            {
                ModelState.AddModelError(fieldName, $"{fieldName} debe ser mayor o igual a {minValue.Value:C}");
                return false;
            }

            if (maxValue.HasValue && value > maxValue.Value)
            {
                ModelState.AddModelError(fieldName, $"{fieldName} debe ser menor o igual a {maxValue.Value:C}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida porcentajes - REPETIDO en AjustePreciosController, ProductosController
        /// </summary>
        protected bool ValidatePercentage(decimal percentage, string fieldName, bool allowZero = true)
        {
            if (!allowZero && percentage <= 0)
            {
                ModelState.AddModelError(fieldName, $"{fieldName} debe ser mayor a 0");
                return false;
            }

            if (allowZero && percentage < 0)
            {
                ModelState.AddModelError(fieldName, $"{fieldName} no puede ser negativo");
                return false;
            }

            if (percentage > 100)
            {
                ModelState.AddModelError(fieldName, $"{fieldName} no puede ser mayor a 100%");
                return false;
            }

            return true;
        }

        #endregion

        #region Validaciones de Negocio Comunes

        /// <summary>
        /// Valida que el usuario pueda realizar la acción - REPETIDO en TODOS los controllers de operaciones
        /// </summary>
        protected async Task<bool> ValidateUserCanPerformActionAsync(string action, object entity = null)
        {
            try
            {
                // Esta lógica se repite en VentasController, ClientesController, etc.
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    ModelState.AddModelError("", "Usuario no autenticado");
                    return false;
                }

                // Validaciones adicionales según el tipo de acción
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error validando permisos de usuario para acción: {Action}", action);
                ModelState.AddModelError("", "Error al validar permisos del usuario");
                return false;
            }
        }

        /// <summary>
        /// Manejo estándar de errores de validación - REPETIDO en TODOS los controllers
        /// </summary>
        protected async Task<IActionResult> HandleValidationFailureAsync<T>(T model, string viewName, Func<T, Task> reloadDataAction = null)
        {
            LogModelStateErrors();

            if (reloadDataAction != null)
            {
                try
                {
                    await reloadDataAction(model);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error recargando datos después de fallo de validación");
                }
            }

            return View(viewName, model);
        }

        #endregion

        #region Helpers para Reutilización

        /// <summary>
        /// Obtiene errores de ModelState como lista - Útil para APIs
        /// </summary>
        protected List<string> GetModelStateErrors()
        {
            return ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();
        }

        /// <summary>
        /// Limpia errores específicos del ModelState
        /// </summary>
        protected void ClearModelStateErrors(params string[] keys)
        {
            foreach (var key in keys)
            {
                ModelState.Remove(key);
            }
        }

        /// <summary>
        /// Validación simplificada para formularios - REPETIDO en todos los controllers
        /// </summary>
        protected async Task<bool> ValidateModelAndHandleErrorsAsync<T>(T model, string viewName, Func<T, Task> reloadDataAction = null)
        {
            if (ModelState.IsValid)
                return true;

            await HandleValidationFailureAsync(model, viewName, reloadDataAction);
            return false;
        }

        #endregion
    }
    #region Validaciones de Negocio Específicas

/// <summary>
/// Valida stock disponible - Se usa en VentasController, DevolucionGarantiaController
/// </summary>
protected async Task<bool> ValidateStockAvailableAsync(int productoId, int cantidadRequerida, IProductoService productoService)
        {
            try
            {
                var producto = await productoService.GetProductoByIDAsync(productoId);
                if (producto == null)
                {
                    ModelState.AddModelError("", $"Producto con ID {productoId} no encontrado");
                    return false;
                }

                var stockDisponible = producto.StockItem?.CantidadDisponible ?? 0;
                if (stockDisponible < cantidadRequerida)
                {
                    ModelState.AddModelError("", $"Stock insuficiente para {producto.Nombre}. Disponible: {stockDisponible}, Requerido: {cantidadRequerida}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error validando stock para producto {ProductoId}", productoId);
                ModelState.AddModelError("", "Error al validar disponibilidad de stock");
                return false;
            }
        }

        /// <summary>
        /// Valida estado de entidad para operación - Se usa en TODOS los controllers de operaciones
        /// </summary>
        protected bool ValidateEntityState<TEnum>(TEnum currentState, TEnum[] allowedStates, string entityName, string operation) where TEnum : Enum
        {
            if (!allowedStates.Contains(currentState))
            {
                var allowedStatesText = string.Join(", ", allowedStates.Select(s => s.ToString()));
                ModelState.AddModelError("", $"No se puede {operation} {entityName} en estado {currentState}. Estados permitidos: {allowedStatesText}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Valida forma de pago - Se usa en VentasController, CotizacionController
        /// </summary>
        protected bool ValidatePaymentMethod(string formaPago, string tipoTarjeta = null, string banco = null)
        {
            if (string.IsNullOrEmpty(formaPago))
            {
                ModelState.AddModelError("FormaPago", "La forma de pago es requerida");
                return false;
            }

            switch (formaPago.ToLower())
            {
                case "tarjeta":
                case "tarjeta crédito":
                case "tarjeta débito":
                    if (string.IsNullOrEmpty(tipoTarjeta))
                    {
                        ModelState.AddModelError("TipoTarjeta", "Debe especificar el tipo de tarjeta");
                        return false;
                    }
                    if (string.IsNullOrEmpty(banco))
                    {
                        ModelState.AddModelError("Banco", "Debe especificar el banco");
                        return false;
                    }
                    break;
                case "transferencia":
                    if (string.IsNullOrEmpty(banco))
                    {
                        ModelState.AddModelError("Banco", "Debe especificar el banco para la transferencia");
                        return false;
                    }
                    break;
            }

            return true;
        }

        /// <summary>
        /// Valida cantidades en operaciones - Se usa en ComprasController, VentasController, etc.
        /// </summary>
        protected bool ValidateQuantities<T>(IEnumerable<T> items, Func<T, int> quantitySelector, string itemName = "items")
        {
            if (items == null || !items.Any())
            {
                ModelState.AddModelError("", $"Debe incluir al menos un {itemName}");
                return false;
            }

            var invalidItems = items.Where(item => quantitySelector(item) <= 0).ToList();
            if (invalidItems.Any())
            {
                ModelState.AddModelError("", $"Las cantidades deben ser mayores a cero para todos los {itemName}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida relaciones de catálogo - Se usa en ProductosController
        /// </summary>
        protected bool ValidateCatalogRelations(int? rubroId, int? subRubroId, int? marcaId)
        {
            if (!rubroId.HasValue || rubroId.Value <= 0)
            {
                ModelState.AddModelError("RubroID", "Debe seleccionar un rubro");
                return false;
            }

            if (subRubroId.HasValue && subRubroId.Value > 0 && (!rubroId.HasValue || rubroId.Value <= 0))
            {
                ModelState.AddModelError("SubRubroID", "No puede seleccionar un subrubro sin seleccionar un rubro");
                return false;
            }

            return true;
        }

        #endregion

        #region Helpers de Validación Asíncrona

        /// <summary>
        /// Ejecuta múltiples validaciones y retorna el resultado consolidado
        /// </summary>
        protected async Task<bool> ExecuteValidationsAsync(params Func<Task<bool>>[] validations)
        {
            var results = await Task.WhenAll(validations.Select(v => v()));
            return results.All(r => r);
        }

        /// <summary>
        /// Valida y prepara modelo para operación
        /// </summary>
        protected async Task<(bool isValid, T processedModel)> ValidateAndPrepareModelAsync<T>(
            T model,
            Func<T, Task> prepareAction = null,
            params Func<Task<bool>>[] additionalValidations) where T : class
        {
            if (!ModelState.IsValid)
            {
                return (false, model);
            }

            var additionalValidationsResult = await ExecuteValidationsAsync(additionalValidations);
            if (!additionalValidationsResult)
            {
                return (false, model);
            }

            if (prepareAction != null)
            {
                await prepareAction(model);
            }

            return (true, model);
        }

        #endregion
    }
}
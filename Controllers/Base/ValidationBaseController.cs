// Controllers/Base/ValidationBaseController.cs
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
}
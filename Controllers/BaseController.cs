// Controllers/Base/BaseController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Javo2.Helpers;
using Javo2.IServices.Authentication;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Javo2.ViewModels;

namespace Javo2.Controllers.Base
{
    public class BaseController : Controller
    {
        protected readonly ILogger _logger;
        protected readonly IPermissionManagerService _permissionManager;

        public BaseController(ILogger logger)
        {
            _logger = logger;
        }

        public BaseController(ILogger logger, IPermissionManagerService permissionManager)
        {
            _logger = logger;
            _permissionManager = permissionManager;
        }

        #region Logging

        protected void LogModelStateErrors()
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogError("ModelState Error en {Key}: {Error}", state.Key, error.ErrorMessage);
                }
            }
        }

        protected void LogInfo(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        protected void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        protected void LogError(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, message, args);
        }

        #endregion

        #region Permission Management

        protected async Task<bool> UserHasPermissionAsync(string permissionCode)
        {
            if (_permissionManager == null)
                return User.HasPermission(permissionCode);

            return await _permissionManager.UserHasPermissionAsync(User, permissionCode);
        }

        protected IActionResult AccessDeniedResult()
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        #endregion

        #region Response Helpers

        protected IActionResult JsonSuccess(string message = null, object data = null)
        {
            return Json(new
            {
                success = true,
                message = message,
                data = data
            });
        }

        protected IActionResult JsonError(string message, object data = null)
        {
            return Json(new
            {
                success = false,
                message = message,
                data = data
            });
        }

        protected void SetSuccessMessage(string message)
        {
            TempData["Success"] = message;
        }

        protected void SetErrorMessage(string message)
        {
            TempData["Error"] = message;
        }

        protected void SetWarningMessage(string message)
        {
            TempData["Warning"] = message;
        }

        protected void SetInfoMessage(string message)
        {
            TempData["Info"] = message;
        }

        #endregion

        #region Common Operations

        protected IActionResult HandleOperationResult(bool result, string successMessage, string errorMessage, object redirectParams = null, string successAction = null, string errorAction = null)
        {
            if (result)
            {
                SetSuccessMessage(successMessage);
                return RedirectToAction(successAction ?? nameof(Index), redirectParams);
            }
            else
            {
                SetErrorMessage(errorMessage);
                return RedirectToAction(errorAction ?? nameof(Index), redirectParams);
            }
        }

        protected async Task<IActionResult> TryExecuteAsync(Func<Task> action, string successMessage, string errorMessage, string successAction = null, string errorAction = null, object redirectParams = null)
        {
            try
            {
                await action();
                SetSuccessMessage(successMessage);
                return RedirectToAction(successAction ?? nameof(Index), redirectParams);
            }
            catch (Exception ex)
            {
                LogError(ex, errorMessage);
                SetErrorMessage($"{errorMessage}: {ex.Message}");
                return RedirectToAction(errorAction ?? nameof(Index), redirectParams);
            }
        }

        protected async Task<IActionResult> TryExecuteWithModelAsync<T>(Func<Task> action, T model, string viewName, string successMessage, string errorMessage, string successAction = null, object redirectParams = null)
        {
            try
            {
                await action();
                SetSuccessMessage(successMessage);
                return RedirectToAction(successAction ?? nameof(Index), redirectParams);
            }
            catch (Exception ex)
            {
                LogError(ex, errorMessage);
                ModelState.AddModelError(string.Empty, $"{errorMessage}: {ex.Message}");
                return View(viewName, model);
            }
        }

        protected async Task<IActionResult> TryHandleModelAsync<T>(T model, Func<T, Task> action, string viewName, string successMessage, string successAction = null, object redirectParams = null)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(viewName, model);
            }

            try
            {
                await action(model);
                SetSuccessMessage(successMessage);
                return RedirectToAction(successAction ?? nameof(Index), redirectParams);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error procesando el modelo: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(viewName, model);
            }
        }

        #endregion

        #region Not Found Handling

        protected async Task<IActionResult> NotFoundIfNullAsync<T>(T entity, string entityName = "Entidad", int? id = null) where T : class
        {
            if (entity == null)
            {
                string errorMessage = id.HasValue
                    ? $"{entityName} con ID {id} no encontrado"
                    : $"{entityName} no encontrado";

                _logger.LogWarning(errorMessage);
                SetErrorMessage(errorMessage);
                return NotFound();
            }

            return null; // Continúa con la ejecución normal
        }

        protected IActionResult HandleNotFoundResult<T>(T entity, string entityName = "Entidad", int? id = null) where T : class
        {
            var result = NotFoundIfNullAsync(entity, entityName, id).Result;
            return result;
        }

        #endregion

        #region Dropdowns Handling

        protected void PopulateSelectList<T>(Dictionary<string, IEnumerable<SelectListItem>> lists, string key, IEnumerable<T> items, Func<T, string> valueSelector, Func<T, string> textSelector, object selectedValue = null)
        {
            var selectList = new List<SelectListItem>();

            foreach (var item in items)
            {
                selectList.Add(new SelectListItem
                {
                    Value = valueSelector(item),
                    Text = textSelector(item),
                    Selected = selectedValue != null && valueSelector(item) == selectedValue.ToString()
                });
            }

            lists[key] = selectList;
        }

        protected SelectList CreateSelectList<T>(IEnumerable<T> items, string dataValueField, string dataTextField, object selectedValue = null)
        {
            return new SelectList(items, dataValueField, dataTextField, selectedValue);
        }

        #endregion
    }
}
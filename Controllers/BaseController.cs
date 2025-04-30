// Controllers/BaseController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Javo2.Helpers;
using Javo2.IServices.Authentication;
using System.Threading.Tasks;
using System.Security.Claims;

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
    }
}
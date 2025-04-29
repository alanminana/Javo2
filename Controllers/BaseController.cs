// Controllers/BaseController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Javo2.Helpers;

namespace Javo2.Controllers.Base
{
    public class BaseController : Controller
    {
        protected readonly ILogger _logger;

        public BaseController(ILogger logger)
        {
            _logger = logger;
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

        protected bool UserHasPermission(string permissionCode)
        {
            return User.HasPermission(permissionCode);
        }

        protected IActionResult AccessDeniedResult()
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
    }
}
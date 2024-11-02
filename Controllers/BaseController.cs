// Controllers/Base/BaseController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
    }
}

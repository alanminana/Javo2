// File: Controllers/AjustePreciosController.cs
using Javo2.IServices;
using Javo2.IServices.Authentication;
using Javo2.ViewModels.Operaciones.Productos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    public class AjustePreciosController : Controller
    {
        private readonly IAjustePrecioService _ajustePrecioService;
        private readonly ILogger<AjustePreciosController> _logger;

        public AjustePreciosController(IAjustePrecioService ajustePrecioService, ILogger<AjustePreciosController> logger)
        {
            _ajustePrecioService = ajustePrecioService;
            _logger = logger;
        }

        // GET: AjustePrecios/Index
        [HttpGet]
        public IActionResult Index()
        {
            return View(); // Se debe crear Views/AjustePrecios/Index.cshtml
        }

        // GET: AjustePrecios/Form
        [HttpGet]
        public IActionResult Form()
        {
            var model = new ConfiguracionIndexViewModel();
            return View(model); // Se debe crear Views/AjustePrecios/Form.cshtml
        }

        // POST: AjustePrecios/Form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Form(ConfiguracionIndexViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                await _ajustePrecioService.AjustarPreciosAsync(model.ProductoIDs, model.Porcentaje, model.EsAumento);
                TempData["Success"] = "Ajuste de precios realizado con éxito.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error al ajustar precios.");
                ModelState.AddModelError("", "Ocurrió un error al ajustar los precios.");
                return View(model);
            }
        }
    }
}

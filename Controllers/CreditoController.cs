// Controllers/CreditoController.cs
using Javo2.Controllers.Base;
using Javo2.IServices;
using Javo2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Javo2.Controllers
{
    [Authorize(Policy = "Permission:configuracion.editar")]
    public class CreditoController : BaseController
    {
        private readonly ICreditoService _creditoService;
        private readonly IClienteService _clienteService;

        public CreditoController(
            ILogger<CreditoController> logger,
            ICreditoService creditoService,
            IClienteService clienteService) : base(logger)
        {
            _creditoService = creditoService;
            _clienteService = clienteService;
        }

        // GET: Credito/Configuracion
        public async Task<IActionResult> Configuracion()
        {
            var config = await _creditoService.GetConfiguracionVigenteAsync();
            return View(config);
        }

        // POST: Credito/Configuracion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Configuracion(ConfiguracionCredito model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.ModificadoPor = User.Identity?.Name ?? "Sistema";
            model.FechaModificacion = DateTime.Now;

            await _creditoService.SaveConfiguracionAsync(model);
            TempData["Success"] = "Configuración de crédito actualizada correctamente";

            return RedirectToAction(nameof(Configuracion));
        }

        // GET: Credito/Criterios
        public async Task<IActionResult> Criterios()
        {
            var criterios = await _creditoService.GetAllCriteriosAsync();
            return View(criterios);
        }

        // GET: Credito/EditarCriterio/A
        public async Task<IActionResult> EditarCriterio(string id)
        {
            var criterio = await _creditoService.GetCriterioByScoreAsync(id);
            if (criterio == null)
            {
                criterio = new CriteriosCalificacionCredito { ScoreCredito = id };
            }

            return View(criterio);
        }

        // POST: Credito/EditarCriterio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCriterio(CriteriosCalificacionCredito model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.ModificadoPor = User.Identity?.Name ?? "Sistema";
            model.FechaModificacion = DateTime.Now;

            await _creditoService.SaveCriterioAsync(model);
            TempData["Success"] = $"Criterio para calificación {model.ScoreCredito} actualizado correctamente";

            return RedirectToAction(nameof(Criterios));
        }
    }
}
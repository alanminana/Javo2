// Archivo: Controllers/HomeController.cs
using Javo2.Controllers.Base;
using Javo2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Javo2.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ILogger<HomeController> logger)
            : base(logger)
        {
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Home Index called");
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy page called");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}

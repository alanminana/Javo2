// File: Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Javo2.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }
        
        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Se debe crear la vista en Views/Account/Login.cshtml
        }
        
        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            // Lógica de autenticación simulada
            if(username == "admin" && password == "admin")
            {
                _logger.LogInformation("Usuario {Username} autenticado correctamente.", username);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Credenciales inválidas.");
                return View();
            }
        }
        
        // GET: Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            _logger.LogInformation("Usuario desconectado.");
            return RedirectToAction("Login");
        }
    }
}

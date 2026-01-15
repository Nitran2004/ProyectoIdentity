using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ProyectoIdentity.Models;
using MercadoPago.Config; // Necesario para MP

using MercadoPago.Client.Preference; // Necesario para MP
using MercadoPago.Resource.Preference;


namespace ProyectoIdentity.Controllers
{
    public class MembresiasController : Controller
    {
        private readonly UserManager<AppUsuario> _userManager;
        private readonly IConfiguration _configuration;

        public MembresiasController(UserManager<AppUsuario> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        // GET: /Membresias
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Membresias/SeleccionarPlan
        [Authorize] // Obligamos a que esté logueado para comprar
        [HttpPost]
        public IActionResult SeleccionarPlan(string plan, decimal precio)
        {
            // Aquí guardaremos la intención de compra y luego 
            // conectaremos con el SDK de Mercado Pago.

            // Por ahora, pasamos los datos al Checkout
            ViewBag.PlanSeleccionado = plan;
            ViewBag.Precio = precio;

            return View("Checkout");
        }
    }
}

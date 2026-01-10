using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIdentity.Datos;
using ProyectoIdentity.Models;
using ProyectoIdentity.Servicios;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProyectoIdentity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger,
                              ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /Home/Index
        public IActionResult Index()
        {
            // Ya no necesitamos cargar productos en el Index
            // porque ahora solo mostramos enlaces a las categorías
            return View();
        }



        //[Authorize(Roles= "Registrado,Administrador")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<string> PruebaCorreo([FromServices] IEmailSender emailSender)
        {
            await emailSender.SendEmailAsync("tu-correo-personal@gmail.com", "Prueba Bitri", "Si lees esto, el SMTP funciona.");
            return "Correo enviado";
        }
    }



    // Clase para manejar requests del chat desde el HomeController
    public class ChatRequest
    {
        public string Mensaje { get; set; } = string.Empty;
    }
}

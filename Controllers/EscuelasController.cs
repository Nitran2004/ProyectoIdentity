using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIdentity.Datos; // Asegúrate que este sea tu namespace de datos
using ProyectoIdentity.Models;

namespace ProyectoIdentity.Controllers
{
    public class EscuelasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EscuelasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listado dinámico por región
        public async Task<IActionResult> Index(string region)
        {
            if (string.IsNullOrEmpty(region)) return RedirectToAction("Index", "Home");

            // Usamos "Escuelas" con S como en tu DbContext
            var listado = await _context.Escuelas
                .Include(e => e.Sedes)
                .Where(e => e.Region == region)
                .ToListAsync();

            ViewData["RegionNombre"] = region;
            return View(listado);
        }

        // Formulario de creación
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Escuela escuela)
        {
            if (ModelState.IsValid)
            {
                _context.Escuelas.Add(escuela);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { region = escuela.Region });
            }
            return View(escuela);
        }
    }
}
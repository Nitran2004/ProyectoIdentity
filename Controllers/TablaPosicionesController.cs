using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIdentity.Datos;
using ProyectoIdentity.Models;

namespace ProyectoIdentity.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class TablaPosicionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TablaPosicionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tabla = await _context.TablaPosiciones
                .OrderBy(x => x.Posicion)
                .ToListAsync();

            return View(tabla);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(TablaPosicion model, IFormFile? archivoImagen)
        {
            if (!ModelState.IsValid)
            {
                var tabla = await _context.TablaPosiciones.OrderBy(x => x.Posicion).ToListAsync();
                return View("Index", tabla);
            }

            if (archivoImagen != null && archivoImagen.Length > 0)
            {
                using var ms = new MemoryStream();
                await archivoImagen.CopyToAsync(ms);
                model.ImagenClub = ms.ToArray();
            }

            _context.TablaPosiciones.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}

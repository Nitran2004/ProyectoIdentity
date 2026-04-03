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
        // GET: Escuelas/Create (Ahora es el panel principal)
        public async Task<IActionResult> Create(int? id)
        {
            // Listado para mostrar abajo de la página
            ViewBag.TodasLasEscuelas = await _context.Escuelas.Include(e => e.Sedes).ToListAsync();

            if (id == null) return View(new Escuela());

            var escuela = await _context.Escuelas.Include(e => e.Sedes).FirstOrDefaultAsync(m => m.Id == id);
            if (escuela == null) return NotFound();

            return View(escuela);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Escuela escuela)
        {
            if (ModelState.IsValid)
            {
                if (escuela.Id == 0)
                {
                    _context.Escuelas.Add(escuela); // Crear nuevo
                }
                else
                {
                    // Para editar: Primero borramos las sedes viejas y subimos las nuevas (evita duplicados)
                    var sedesViejas = _context.SedesEscuelas.Where(s => s.EscuelaId == escuela.Id);
                    _context.SedesEscuelas.RemoveRange(sedesViejas);

                    _context.Update(escuela); // Actualizar escuela y sus nuevas sedes
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }
            ViewBag.TodasLasEscuelas = await _context.Escuelas.Include(e => e.Sedes).ToListAsync();
            return View(escuela);
        }

        // Acción para eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var escuela = await _context.Escuelas.FindAsync(id);
            if (escuela != null)
            {
                _context.Escuelas.Remove(escuela);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Create));
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIdentity.Models;
using ProyectoIdentity.Datos;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoIdentity.Controllers
{
    public class UbicacionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UbicacionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ubicaciones
        public async Task<IActionResult> Index(string buscarCiudad)
        {
            // Consulta base: solo activos
            var consulta = _context.Ubicaciones.Where(u => u.Activo);

            // Filtro por ciudad si el usuario escribe algo
            if (!string.IsNullOrEmpty(buscarCiudad))
            {
                consulta = consulta.Where(u => u.Ciudad.Contains(buscarCiudad));
            }

            var ubicaciones = await consulta.OrderBy(u => u.Nombre).ToListAsync();

            // Guardamos el termino buscado para mostrarlo en el input de la vista
            ViewData["FiltroActual"] = buscarCiudad;

            return View(ubicaciones);
        }

        // GET: Ubicaciones/Crear (Sirve para Crear y Editar)
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear(int? id)
        {
            if (id == null || id == 0)
            {
                // Modo Creacion
                return View(new Ubicacion());
            }

            // Modo Edicion
            var ubicacion = await _context.Ubicaciones.FindAsync(id);
            if (ubicacion == null || !ubicacion.Activo)
            {
                return NotFound();
            }

            return View(ubicacion);
        }

        // POST: Ubicaciones/Crear (Maneja el Insert y el Update)

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Ubicacion ubicacion)
        {
            if (ModelState.IsValid)
            {
                if (ubicacion.Id == 0)
                {
                    // Insertar
                    ubicacion.FechaCreacion = DateTime.Now;
                    ubicacion.Activo = true;
                    _context.Add(ubicacion);
                }
                else
                {
                    // Actualizar
                    var ubicacionDb = await _context.Ubicaciones.AsNoTracking().FirstOrDefaultAsync(u => u.Id == ubicacion.Id);
                    if (ubicacionDb == null) return NotFound();

                    ubicacion.FechaCreacion = ubicacionDb.FechaCreacion; // Mantenemos la fecha original
                    ubicacion.Activo = true;

                    _context.Update(ubicacion);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(ubicacion);
        }

        // POST: Ubicaciones/Borrar/5
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrar(int id)
        {
            var ubicacion = await _context.Ubicaciones.FindAsync(id);
            if (ubicacion != null)
            {
                ubicacion.Activo = false; // Borrado logico
                _context.Update(ubicacion);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
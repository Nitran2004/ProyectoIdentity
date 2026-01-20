using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIdentity.Datos;
using ProyectoIdentity.Models;
using System.IO;
using System.Threading.Tasks;

[Authorize(Roles = "Administrador")]
public class PartidosController : Controller
{
    private readonly ApplicationDbContext _context;

    public PartidosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View(new Partido { FechaHora = System.DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Partido partido, IFormFile? archivoLocal, IFormFile? archivoVisitante)
    {
        if (!ModelState.IsValid)
            return View(partido);

        var existente = await _context.Partidos
            .OrderBy(p => p.Id)
            .FirstOrDefaultAsync();

        if (archivoLocal != null && archivoLocal.Length > 0)
        {
            using var ms = new MemoryStream();
            await archivoLocal.CopyToAsync(ms);
            partido.ImagenLocal = ms.ToArray();
        }
        else if (existente != null)
        {
            partido.ImagenLocal = existente.ImagenLocal; // conserva si no sube nueva
        }

        if (archivoVisitante != null && archivoVisitante.Length > 0)
        {
            using var ms = new MemoryStream();
            await archivoVisitante.CopyToAsync(ms);
            partido.ImagenVisitante = ms.ToArray();
        }
        else if (existente != null)
        {
            partido.ImagenVisitante = existente.ImagenVisitante; // conserva si no sube nueva
        }

        if (existente == null)
        {
            _context.Partidos.Add(partido);
        }
        else
        {
            existente.FechaHora = partido.FechaHora;
            existente.Local = partido.Local;
            existente.Visitante = partido.Visitante;
            existente.Estadio = partido.Estadio;
            existente.ImagenLocal = partido.ImagenLocal;
            existente.ImagenVisitante = partido.ImagenVisitante;

            _context.Partidos.Update(existente);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Home");
    }

}

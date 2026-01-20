using Microsoft.AspNetCore.Mvc;
using ProyectoIdentity.Datos;
using ProyectoIdentity.Models;
using System.Linq;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var partido = _context.Partidos
            .OrderByDescending(p => p.Id)
            .FirstOrDefault();

        var tabla = _context.TablaPosiciones
            .OrderBy(t => t.Posicion)
            .ToList();

        var vm = new HomeIndexViewModel
        {
            ProximoPartido = partido,
            Tabla = tabla
        };

        return View(vm);
    }
}

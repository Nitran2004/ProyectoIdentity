using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIdentity.Datos;
using ProyectoIdentity.Models;
using System.Security.Claims;

namespace ProyectoIdentity.Controllers
{
    [Authorize]
    public class SocioController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly UserManager<AppUsuario> _userManager;
        public SocioController(ApplicationDbContext contexto, UserManager<AppUsuario> userManager)
        {
            _contexto = contexto;
            _userManager = userManager;
        }

        // Dentro de SocioController.cs
        [HttpGet]
        public async Task<IActionResult> Home()
        {
            // Obtener el ID del usuario actual
            var userId = _userManager.GetUserId(User);

            // Buscar al usuario en la BD
            var usuario = await _contexto.AppUsuario
                .FirstOrDefaultAsync(u => u.Id == userId);

            // BUSCAR EL PARTIDO (Igual que en el Index)
            var proximoPartido = await _contexto.Partidos
                .Where(p => p.FechaHora >= DateTime.Now)
                .OrderBy(p => p.FechaHora)
                .FirstOrDefaultAsync();

            // PASARLO AL VIEWBAG
            ViewBag.ProximoPartido = proximoPartido;

            return View(usuario);
        }

        // ACCIÓN PARA EL CARNET DIGITAL
        public async Task<IActionResult> Carnet()
        {
            // Obtener el usuario actual con todos sus datos personalizados
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
            {
                return NotFound("No se encontró el usuario.");
            }

            // Pasamos el modelo usuario a la vista Carnet.cshtml
            return View(usuario);
        }
    }
}

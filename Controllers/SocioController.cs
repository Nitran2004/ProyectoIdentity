using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoIdentity.Models;

namespace ProyectoIdentity.Controllers
{
    [Authorize]
    public class SocioController : Controller
    {
        private readonly UserManager<AppUsuario> _userManager;

        public SocioController(UserManager<AppUsuario> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Home()
        {
            var usuario = await _userManager.GetUserAsync(User);
            return View(usuario);
        }
    }
}

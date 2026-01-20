using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoIdentity.Models;
using System.Runtime.InteropServices;

namespace ProyectoIdentity.Controllers
{
    [Authorize]
    public class CuentasController : Controller
    {
        // Antes decía IdentityUser, cámbialo a AppUsuario:
        private readonly UserManager<AppUsuario> _userManager;
        private readonly SignInManager<AppUsuario> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;

        public CuentasController(UserManager<AppUsuario> userManager,
                                 SignInManager<AppUsuario> signInManager,
                                 IEmailSender emailSender,
                                 RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Método para crear todos los roles necesarios en la aplicación
        /// </summary>
        private async Task CrearRolesAsync()
        {
            // Crear rol Administrador
            if (!await _roleManager.RoleExistsAsync("Administrador"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Administrador"));
            }

            // Crear rol Cajero
            if (!await _roleManager.RoleExistsAsync("Platino"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Platino"));
            }

            // Crear rol Registrado
            if (!await _roleManager.RoleExistsAsync("Oro"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Oro"));
            }

            // Crear rol Registrado
            if (!await _roleManager.RoleExistsAsync("Plata"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Plata"));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(string returnurl = null)
        {
            // Crear todos los roles al inicio de la aplicación
            await CrearRolesAsync();

            ViewData["ReturnUrl"] = returnurl;
            RegistroViewModel registroVM = new RegistroViewModel();
            return View(registroVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel rgViewModel, string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var usuario = new AppUsuario
                {
                    UserName = rgViewModel.Email,
                    Email = rgViewModel.Email,
                    Nombre = rgViewModel.Nombre,
                    Cedula = rgViewModel.Cedula,
                    Telefono = rgViewModel.Telefono,
                    Pais = rgViewModel.Pais,
                    Ciudad = rgViewModel.Ciudad,
                    Direccion = rgViewModel.Direccion,
                    FechaNacimiento = rgViewModel.FechaNacimiento
                };
                var resultado = await _userManager.CreateAsync(usuario, rgViewModel.Password);
                if (resultado.Succeeded)
                {
                    await _userManager.AddToRoleAsync(usuario, "Registrado");

                    // --- INICIO DE LÓGICA DE VALIDACIÓN ---

                    // 1. Generar el token de confirmación
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);

                    // 2. Crear el link que apunta a una nueva acción 'ConfirmarEmail'
                    var callbackUrl = Url.Action("ConfirmarEmail", "Cuentas",
                        new { userId = usuario.Id, code = code }, Request.Scheme);

                    // 3. Diseño del correo (Estilo El Nacional)
                    string cuerpoHtml = $@"
                <div style='font-family: Arial; border: 1px solid #ddd; padding: 20px; max-width: 600px;'>
                    <h2 style='color: #DA291C;'>¡Bienvenido al Bitri!</h2>
                    <p>Gracias por registrarte. Para activar tu cuenta, por favor confirma tu correo:</p>
                    <a href='{callbackUrl}' style='background-color: #111; color: white; padding: 10px 20px; text-decoration: none;'>
                        CONFIRMAR MI CUENTA
                    </a>
                </div>";

                    // 4. Enviar el correo
                    await _emailSender.SendEmailAsync(rgViewModel.Email, "Confirma tu cuenta - El Nacional", cuerpoHtml);

                    // 5. IMPORTANTE: NO hacemos SignInAsync. Redirigimos a una página de aviso.
                    return View("RegistroConfirmacionEspera");

                    // --- FIN DE LÓGICA DE VALIDACIÓN ---
                }
                ValidarErrores(resultado);
            }
            return View(rgViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ReenviarConfirmacionEmail(string email)
        {
            return View(new OlvidoPasswordViewModel { Email = email });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReenviarConfirmacionEmail(OlvidoPasswordViewModel model)
        {
            var usuario = await _userManager.FindByEmailAsync(model.Email);
            if (usuario == null) return View("RegistroConfirmacionEspera");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(usuario);
            var callbackUrl = Url.Action("ConfirmarEmail", "Cuentas", new { userId = usuario.Id, code = code }, Request.Scheme);

            await _emailSender.SendEmailAsync(model.Email, "Confirma tu cuenta - El Nacional",
                $"Por favor confirma tu cuenta haciendo clic aquí: <a href='{callbackUrl}'>link</a>");

            return View("RegistroConfirmacionEspera");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                return View("Error");
            }

            // Aquí Identity marca el campo 'EmailConfirmed' como TRUE en la base de datos
            var resultado = await _userManager.ConfirmEmailAsync(usuario, code);

            if (resultado.Succeeded)
            {
                return View(); // Crea una vista que diga "Cuenta activada con éxito"
            }

            return View("Error");
        }

        // Registro especial solo para administradores
        [HttpGet]
        public async Task<IActionResult> RegistroAdministrador(string returnurl = null)
        {
            // Crear todos los roles
            await CrearRolesAsync();

            // Lista de roles disponibles para selección
            List<SelectListItem> listaRoles = new List<SelectListItem>
            {
                new SelectListItem()
                {
                    Value = "Registrado",
                    Text = "Registrado"
                },
                new SelectListItem()
                {
                    Value = "Platino",
                    Text = "Platino"
                },
                new SelectListItem()
                {
                    Value = "Oro",
                    Text = "Oro"
                },
                new SelectListItem()
                {
                    Value = "Plata",
                    Text = "Plata"
                }
            };

            ViewData["ReturnUrl"] = returnurl;
            RegistroViewModel registroVM = new RegistroViewModel()
            {
                ListaRoles = listaRoles
            };

            return View(registroVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroAdministrador(RegistroViewModel rgViewModel, string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var usuario = new AppUsuario
                {
                    UserName = rgViewModel.Email,
                    Email = rgViewModel.Email,
                    Nombre = rgViewModel.Nombre,
                    Cedula = rgViewModel.Cedula,
                    Telefono = rgViewModel.Telefono,
                    Pais = rgViewModel.Pais,
                    Ciudad = rgViewModel.Ciudad,
                    Direccion = rgViewModel.Direccion,
                    FechaNacimiento = rgViewModel.FechaNacimiento
                };

                var resultado = await _userManager.CreateAsync(usuario, rgViewModel.Password);

                if (resultado.Succeeded)
                {
                    // Asignar el rol seleccionado o por defecto "Registrado"
                    if (!string.IsNullOrEmpty(rgViewModel.RolSeleccionado))
                    {
                        // Validar que el rol seleccionado sea uno de los permitidos
                        if (rgViewModel.RolSeleccionado == "Administrador" ||
                            rgViewModel.RolSeleccionado == "Platino" ||
                            rgViewModel.RolSeleccionado == "Oro"||
                            rgViewModel.RolSeleccionado == "Plata" )


                        {
                            await _userManager.AddToRoleAsync(usuario, rgViewModel.RolSeleccionado);
                        }
                        else
                        {
                            // Si el rol no es válido, asignar "Registrado" por defecto
                            await _userManager.AddToRoleAsync(usuario, "Registrado");
                        }
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(usuario, "Registrado");
                    }

                    await _signInManager.SignInAsync(usuario, isPersistent: false);
                    return LocalRedirect(returnurl);
                }

                ValidarErrores(resultado);
            }

            // Recargar la lista de roles en caso de error
            rgViewModel.ListaRoles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Registrado", Text = "Registrado" },
                new SelectListItem { Value = "Administrador", Text = "Administrador" },
                new SelectListItem { Value = "Platino", Text = "Platino" },
                new SelectListItem { Value = "Oro", Text = "Oro" },
                new SelectListItem { Value = "Plata", Text = "Plata" }
            };

            return View(rgViewModel);
        }

        [AllowAnonymous]
        private void ValidarErrores(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(String.Empty, error.Description);
            }
        }

        // Método mostrar formulario de acceso
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Acceso(string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Acceso(AccesoViewModel accViewModel, string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(accViewModel.Email);
                if (usuario != null && !await _userManager.IsEmailConfirmedAsync(usuario))
                {
                    ModelState.AddModelError(string.Empty, "Debes confirmar tu correo electrónico antes de iniciar sesión.");
                    return View(accViewModel);
                }
                var resultado = await _signInManager.PasswordSignInAsync(accViewModel.Email, accViewModel.Password, accViewModel.RememberMe, lockoutOnFailure: true);

                if (resultado.Succeeded)
                {
                    return LocalRedirect(returnurl);
                }
                if (resultado.IsLockedOut)
                {
                    return View("Bloqueado");
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Acceso invalido");
                    return View(accViewModel);
                }
            }

            return View(accViewModel);
        }

        // Salir o cerrar sesion de la aplicación (logout)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalirAplicacion()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // Método para olvido de contraseña
        [HttpGet]
        [AllowAnonymous]
        public IActionResult OlvidoPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous] // Importante para que alguien que no ha entrado pueda recuperarla
        public async Task<IActionResult> OlvidoPassword(OlvidoPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(model.Email);
                if (usuario == null)
                {
                    return RedirectToAction("ConfirmacionOlvidoPassword");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

                // OJO: Aquí usamos "ResetPassword" porque así se llama tu método GET
                // Y usamos "code" porque así se llama la propiedad en tu RecuperaPasswordViewModel
                var callbackUrl = Url.Action("ResetPassword", "Cuentas",
                    new { code = token, email = model.Email }, Request.Scheme);

                string cuerpoHtml = $@"<p>Haz clic <a href='{callbackUrl}'>aquí</a> para restablecer tu clave.</p>";

                // CORRECCIÓN DEL ERROR CS0103:
                // Usamos _emailSender que es como lo tienes en el constructor
                await _emailSender.SendEmailAsync(model.Email, "Restablecer Contraseña", cuerpoHtml);

                return RedirectToAction("ConfirmacionOlvidoPassword");
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmacionOlvidoPassword()
        {
            return View();
        }

        // Funcionalidad para recuperar contraseña
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null, string email = null)
        {
            if (code == null) return View("Error");

            // Pasamos el email y el token a la vista automáticamente
            var modelo = new RecuperaPasswordViewModel { Code = code, Email = email };
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(RecuperaPasswordViewModel rpViewModel)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(rpViewModel.Email);
                if (usuario == null)
                {
                    return RedirectToAction("ConfirmacionRecuperaPassword");
                }

                var resultado = await _userManager.ResetPasswordAsync(usuario, rpViewModel.Code, rpViewModel.Password);

                if (resultado.Succeeded)
                {
                    return RedirectToAction("ConfirmacionRecuperaPassword");
                }

                ValidarErrores(resultado);
            }
            return View(rpViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmacionRecuperaPassword()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Denegado(string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            return View();
        }
     }
}
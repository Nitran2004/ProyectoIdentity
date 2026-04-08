using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoIdentity.Datos;
using ProyectoIdentity.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ProyectoIdentity.Controllers
{
    public class MembresiasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUsuario> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MembresiasController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public MembresiasController(
            ApplicationDbContext context,
            UserManager<AppUsuario> userManager,
            IConfiguration configuration,
            ILogger<MembresiasController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private async Task<string> GetAccessToken()
        {
            var clientId = _configuration["PayPal:ClientId"];
            var clientSecret = _configuration["PayPal:ClientSecret"];
            var mode = _configuration["PayPal:Mode"];

            var baseUrl = mode == "sandbox"
                ? "https://api-m.sandbox.paypal.com"
                : "https://api-m.paypal.com";

            var client = _httpClientFactory.CreateClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            var response = await client.PostAsync(
                $"{baseUrl}/v1/oauth2/token",
                new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") })
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(result);

            return json.GetProperty("access_token").GetString()!;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var usuario = await _userManager.GetUserAsync(User);
                ViewBag.MembresiaActual = usuario?.TipoMembresia;
                ViewBag.EstadoMembresia = usuario?.EstadoMembresia;
                ViewBag.FechaVencimiento = usuario?.FechaFinMembresia;
            }

            return View();
        }

        //Pay pal sandbox 
        //[Authorize]
        //public async Task<IActionResult> MiMembresia()
        //{
        //    var usuario = await _userManager.GetUserAsync(User);

        //    var suscripcion = await _context.SuscripcionesUsuario
        //        .Include(s => s.Plan)
        //        .Where(s => s.UsuarioId == usuario.Id && s.EsActual)
        //        .FirstOrDefaultAsync();

        //    return View(suscripcion);
        //}

        public async Task<IActionResult> MiMembresia(string plan, string paypalPlanId, decimal? precio)
        {
            var usuario = await _userManager.GetUserAsync(User);

            var suscripcion = await _context.SuscripcionesUsuario
                .Include(s => s.Plan)
                .Where(s => s.UsuarioId == usuario.Id && s.EsActual)
                .FirstOrDefaultAsync();

            ViewBag.PlanSeleccionado = plan;
            ViewBag.PayPalPlanId = paypalPlanId;
            ViewBag.PrecioPlan = precio;          // ← NUEVO: pasa el precio a la vista

            return View(suscripcion);
        }

        // =========================
        // 1) Confirmar desde BOTÓN (PayPal Button Factory)
        // =========================
        public class ConfirmarSuscripcionDto
        {
            public string? PayPalSuscripcionId { get; set; }
            public string? PayPalPlanId { get; set; }
            public string? TipoMembresia { get; set; } // "Plata", "Oro", etc. (opcional)
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmarSuscripcionPayPal([FromBody] ConfirmarSuscripcionDto dto)
        {
            try
            {
                if (dto == null ||
                    string.IsNullOrWhiteSpace(dto.PayPalSuscripcionId) ||
                    string.IsNullOrWhiteSpace(dto.PayPalPlanId))
                {
                    return Json(new { ok = false, mensaje = "Datos incompletos." });
                }

                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null) return Json(new { ok = false, mensaje = "Usuario no encontrado." });

                var accessToken = await GetAccessToken();
                var mode = _configuration["PayPal:Mode"];
                var baseUrl = mode == "sandbox"
                    ? "https://api-m.sandbox.paypal.com"
                    : "https://api-m.paypal.com";

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Validar suscripción en PayPal
                var response = await client.GetAsync($"{baseUrl}/v1/billing/subscriptions/{dto.PayPalSuscripcionId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No se pudo validar suscripción en PayPal. Status: {status}", response.StatusCode);
                    return Json(new { ok = false, mensaje = "No se pudo validar la suscripción en PayPal." });
                }

                var contenido = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(contenido);

                var estado = json.GetProperty("status").GetString();       // ACTIVE / APPROVAL_PENDING / etc
                var planId = json.GetProperty("plan_id").GetString();      // P-XXXX

                if (!string.Equals(planId, dto.PayPalPlanId, StringComparison.OrdinalIgnoreCase))
                    return Json(new { ok = false, mensaje = "El plan no coincide con el esperado." });

                if (estado != "ACTIVE" && estado != "APPROVAL_PENDING")
                    return Json(new { ok = false, mensaje = $"La suscripción no está activa (estado: {estado})." });

                // Intentar leer next_billing_time (a veces viene vacío al inicio dependiendo del estado)
                DateTime? proximaFacturacion = null;
                if (json.TryGetProperty("billing_info", out var billingInfo) &&
                    billingInfo.TryGetProperty("next_billing_time", out var nextBillingEl) &&
                    nextBillingEl.ValueKind == JsonValueKind.String)
                {
                    if (DateTime.TryParse(nextBillingEl.GetString(), out var dt))
                        proximaFacturacion = dt.ToLocalTime();
                }

                // Guardar en AppUsuario (rápido y útil)
                usuario.PayPalSuscripcionId = dto.PayPalSuscripcionId;
                usuario.PayPalPlanId = dto.PayPalPlanId;
                usuario.TipoMembresia = dto.TipoMembresia ?? usuario.TipoMembresia ?? "Plata";
                usuario.EstadoMembresia = "Activa";
                usuario.FechaInicioMembresia = DateTime.Now;
                usuario.FechaFinMembresia = proximaFacturacion; // puede quedar null si PayPal no lo devuelve aún

                await _userManager.UpdateAsync(usuario);

                // Si estás usando tus tablas (PlanesMembresia/SuscripcionesUsuario), también guardamos ahí:
                // OJO: Aquí asumimos que tu plan está en BD y puedes mapearlo por nombre o por id.
                // Si prefieres, puedes OMITIR esta parte y quedarte solo con AppUsuario.
                var suscripcionActual = await _context.SuscripcionesUsuario
                    .Where(s => s.UsuarioId == usuario.Id && s.EsActual)
                    .FirstOrDefaultAsync();

                if (suscripcionActual != null)
                {
                    suscripcionActual.EsActual = false;
                    suscripcionActual.FechaActualizacion = DateTime.UtcNow;
                }

                // Si tienes un plan "Plata" en tu tabla PlanesMembresia:
                var planDb = await _context.PlanesMembresia
                    .Where(p => p.Activo && p.Nombre == (dto.TipoMembresia ?? "Plata"))
                    .FirstOrDefaultAsync();

                if (planDb != null)
                {
                    var nueva = new SuscripcionUsuario
                    {
                        UsuarioId = usuario.Id,
                        PlanMembresiaId = planDb.Id,
                        PayPalSuscripcionId = dto.PayPalSuscripcionId,
                        Estado = "ACTIVA",
                        FechaInicio = DateTime.UtcNow,
                        EsActual = true,
                        FechaActualizacion = DateTime.UtcNow
                    };

                    _context.SuscripcionesUsuario.Add(nueva);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Si no existe el plan en BD, no fallamos: igual dejamos AppUsuario actualizado.
                    _logger.LogWarning("No se encontró plan en BD con Nombre={nombre}. Se guardó solo en AppUsuario.", dto.TipoMembresia);
                    await _context.SaveChangesAsync();
                }

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ConfirmarSuscripcionPayPal");
                return Json(new { ok = false, mensaje = "Error interno al confirmar la suscripción." });
            }
        }

        // =========================
        // 2) (Opcional) GuardarSuscripcion si lo sigues usando desde otra vista
        // =========================
        public class GuardarSuscripcionDto
        {
            public int PlanId { get; set; }
            public string PayPalSubscriptionId { get; set; } = null!;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GuardarSuscripcion([FromBody] GuardarSuscripcionDto dto)
        {
            var usuario = await _userManager.GetUserAsync(User);

            var plan = await _context.PlanesMembresia.FirstOrDefaultAsync(p => p.Id == dto.PlanId && p.Activo);
            if (plan == null) return BadRequest("Plan no válido.");

            var actual = await _context.SuscripcionesUsuario
                .Where(s => s.UsuarioId == usuario.Id && s.EsActual)
                .FirstOrDefaultAsync();

            if (actual != null)
            {
                actual.EsActual = false;
                actual.FechaActualizacion = DateTime.UtcNow;
            }

            var nueva = new SuscripcionUsuario
            {
                UsuarioId = usuario.Id,
                PlanMembresiaId = plan.Id,
                PayPalSuscripcionId = dto.PayPalSubscriptionId,
                Estado = "ACTIVA",
                FechaInicio = DateTime.UtcNow,
                EsActual = true,
                FechaActualizacion = DateTime.UtcNow
            };

            _context.SuscripcionesUsuario.Add(nueva);
            await _context.SaveChangesAsync();

            // También reflejamos en AppUsuario
            usuario.TipoMembresia = plan.Nombre;
            usuario.EstadoMembresia = "Activa";
            usuario.FechaInicioMembresia = DateTime.Now;
            await _userManager.UpdateAsync(usuario);

            return Ok(new { mensaje = "Suscripción guardada", plan = plan.Nombre });
        }

        // =========================
        // 3) Cancelar
        // =========================
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CancelarMembresia()
        {
            try
            {
                var usuario = await _userManager.GetUserAsync(User);

                var suscripcion = await _context.SuscripcionesUsuario
                    .Where(s => s.UsuarioId == usuario.Id && s.EsActual && s.Estado == "ACTIVA")
                    .FirstOrDefaultAsync();

                if (suscripcion == null || string.IsNullOrEmpty(suscripcion.PayPalSuscripcionId))
                {
                    // Si no existe en tabla, igual intentamos con AppUsuario (por si solo guardaste ahí)
                    if (!string.IsNullOrEmpty(usuario.PayPalSuscripcionId))
                    {
                        suscripcion = new SuscripcionUsuario { PayPalSuscripcionId = usuario.PayPalSuscripcionId };
                    }
                    else
                    {
                        TempData["Error"] = "No se encontró suscripción activa";
                        return RedirectToAction("MiMembresia");
                    }
                }

                var accessToken = await GetAccessToken();
                var mode = _configuration["PayPal:Mode"];
                var baseUrl = mode == "sandbox"
                    ? "https://api-m.sandbox.paypal.com"
                    : "https://api-m.paypal.com";

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var cancelData = new { reason = "Usuario canceló la membresía" };

                var response = await client.PostAsync(
                    $"{baseUrl}/v1/billing/subscriptions/{suscripcion.PayPalSuscripcionId}/cancel",
                    new StringContent(JsonSerializer.Serialize(cancelData), Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
                    // Actualizar tabla SuscripcionesUsuario si existe
                    var suscripcionDb = await _context.SuscripcionesUsuario
                        .Where(s => s.UsuarioId == usuario.Id && s.EsActual && s.PayPalSuscripcionId == suscripcion.PayPalSuscripcionId)
                        .FirstOrDefaultAsync();

                    if (suscripcionDb != null)
                    {
                        suscripcionDb.Estado = "CANCELADA";
                        suscripcionDb.FechaCancelacion = DateTime.UtcNow;
                        suscripcionDb.FechaFin = DateTime.UtcNow;
                        suscripcionDb.EsActual = false;
                        suscripcionDb.FechaActualizacion = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }

                    // Actualizar AppUsuario
                    usuario.EstadoMembresia = "Cancelada";
                    usuario.FechaFinMembresia = DateTime.Now;
                    await _userManager.UpdateAsync(usuario);

                    TempData["Mensaje"] = "Membresía cancelada exitosamente";
                }
                else
                {
                    TempData["Error"] = "Error al cancelar la membresía";
                }

                return RedirectToAction("MiMembresia");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar");
                TempData["Error"] = "Error al cancelar la membresía";
                return RedirectToAction("MiMembresia");
            }
        }

        //[Authorize]
        //[HttpPost]
        //public async Task<IActionResult> SeleccionarPlan(string plan, decimal precio)
        //{
        //    try
        //    {
        //        var usuario = await _userManager.GetUserAsync(User);
        //        if (usuario == null) return Unauthorized();

        //        var accessToken = await GetAccessToken();

        //        var mode = _configuration["PayPal:Mode"];
        //        var baseUrl = mode == "sandbox"
        //            ? "https://api-m.sandbox.paypal.com"
        //            : "https://api-m.paypal.com";

        //        var client = _httpClientFactory.CreateClient();
        //        client.DefaultRequestHeaders.Authorization =
        //            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        //        // 🔹 CREAR PRODUCTO
        //        var producto = new
        //        {
        //            name = $"Membresía {plan}",
        //            type = "SERVICE"
        //        };

        //        var productoResponse = await client.PostAsync(
        //            $"{baseUrl}/v1/catalogs/products",
        //            new StringContent(JsonSerializer.Serialize(producto), Encoding.UTF8, "application/json")
        //        );

        //        var productoJson = JsonSerializer.Deserialize<JsonElement>(
        //            await productoResponse.Content.ReadAsStringAsync()
        //        );

        //        var productoId = productoJson.GetProperty("id").GetString();

        //        // 🔹 CREAR PLAN
        //        // 🔹 CREAR PLAN
        //        var planData = new
        //        {
        //            product_id = productoId,
        //            name = $"Plan {plan}",
        //            description = $"Suscripción mensual {plan}",
        //            status = "ACTIVE",
        //            billing_cycles = new[]
        //            {
        //                new
        //                {
        //                    frequency = new
        //                    {
        //                        interval_unit = "MONTH",
        //                        interval_count = 1
        //                    },
        //                    tenure_type = "REGULAR",
        //                    sequence = 1,
        //                    total_cycles = 0,
        //                    pricing_scheme = new
        //                    {
        //                        fixed_price = new
        //                        {
        //                            value = precio.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
        //                            currency_code = "USD"
        //                        }
        //                    }
        //                }
        //            },
        //            payment_preferences = new
        //            {
        //                auto_bill_outstanding = true,
        //                setup_fee_failure_action = "CONTINUE",
        //                payment_failure_threshold = 3
        //            }
        //        };

        //        // ✅ PRIMERO haces el POST
        //        var planResponse = await client.PostAsync(
        //            $"{baseUrl}/v1/billing/plans",
        //            new StringContent(JsonSerializer.Serialize(planData), Encoding.UTF8, "application/json")
        //        );

        //        // ✅ LUEGO lees la respuesta
        //        var planResult = await planResponse.Content.ReadAsStringAsync();

        //        // ✅ VALIDAS ERROR
        //        if (!planResponse.IsSuccessStatusCode)
        //        {
        //            _logger.LogError("❌ Error creando plan PayPal: " + planResult);
        //            throw new Exception("Error PayPal: " + planResult);
        //        }

        //        // ✅ PARSEAS JSON
        //        var planJson = JsonSerializer.Deserialize<JsonElement>(planResult);
        //        var planId = planJson.GetProperty("id").GetString();

        //        // 🔹 CREAR SUSCRIPCIÓN
        //        var suscripcion = new
        //        {
        //            plan_id = planId,
        //            application_context = new
        //            {
        //                return_url = Url.Action(
        //                "Resultado",
        //                "Membresias",
        //                new { plan = plan },
        //                Request.Scheme
        //            ),
        //                cancel_url = _configuration["PayPal:CancelUrl"]
        //            }
        //        };

        //        var suscripcionResponse = await client.PostAsync(
        //            $"{baseUrl}/v1/billing/subscriptions",
        //            new StringContent(JsonSerializer.Serialize(suscripcion), Encoding.UTF8, "application/json")
        //        );

        //        var suscripcionResult = await suscripcionResponse.Content.ReadAsStringAsync();

        //        if (!suscripcionResponse.IsSuccessStatusCode)
        //        {
        //            _logger.LogError("❌ Error creando suscripción: " + suscripcionResult);
        //            throw new Exception("Error PayPal: " + suscripcionResult);
        //        }

        //        var suscripcionJson = JsonSerializer.Deserialize<JsonElement>(suscripcionResult);

        //        var approveLink = suscripcionJson.GetProperty("links")
        //            .EnumerateArray()
        //            .First(l => l.GetProperty("rel").GetString() == "approve")
        //            .GetProperty("href").GetString();

        //        // 🔥 REDIRECCIÓN A PAYPAL PILAS
        //        //return Redirect(approveLink);

        //        TempData["PayPalLink"] = approveLink;

        //        // 👉 Redirigimos a tu flujo normal
        //        return RedirectToAction("MiMembresia");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error en SeleccionarPlan");
        //        TempData["Error"] = "Error al procesar el pago";
        //        return RedirectToAction("Index");
        //    }
        //}

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SeleccionarPlan(string plan, decimal precio)
        {
            try
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null) return Unauthorized();

                var accessToken = await GetAccessToken();

                var mode = _configuration["PayPal:Mode"];
                var baseUrl = mode == "sandbox"
                    ? "https://api-m.sandbox.paypal.com"
                    : "https://api-m.paypal.com";

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                // 🔹 CREAR PRODUCTO
                var producto = new
                {
                    name = $"Membresía {plan}",
                    type = "SERVICE"
                };

                var productoResponse = await client.PostAsync(
                    $"{baseUrl}/v1/catalogs/products",
                    new StringContent(JsonSerializer.Serialize(producto), Encoding.UTF8, "application/json")
                );

                var productoJson = JsonSerializer.Deserialize<JsonElement>(
                    await productoResponse.Content.ReadAsStringAsync()
                );

                var productoId = productoJson.GetProperty("id").GetString();

                // 🔹 CREAR PLAN
                var planData = new
                {
                    product_id = productoId,
                    name = $"Plan {plan}",
                    description = $"Suscripción mensual {plan}",
                    status = "ACTIVE",

                    billing_cycles = new[]
                    {
        new
        {
            frequency = new
            {
                interval_unit = "MONTH",
                interval_count = 1
            },
            tenure_type = "REGULAR",
            sequence = 1,
            total_cycles = 0,
            pricing_scheme = new
            {
                fixed_price = new
                {
                    value = precio.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    currency_code = "USD"
                }
            }
        }
    },

                    payment_preferences = new
                    {
                        auto_bill_outstanding = true,
                        setup_fee_failure_action = "CONTINUE",
                        payment_failure_threshold = 3
                    },

                    taxes = new
                    {
                        percentage = "0",
                        inclusive = false
                    }
                };

                var planResponse = await client.PostAsync(
                    $"{baseUrl}/v1/billing/plans",
                    new StringContent(JsonSerializer.Serialize(planData), Encoding.UTF8, "application/json")
                );

                var planResult = await planResponse.Content.ReadAsStringAsync();

                if (!planResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ ERROR PAYPAL PLAN: " + planResult);
                    throw new Exception(planResult);
                }

                var planJson = JsonSerializer.Deserialize<JsonElement>(planResult);
                var planId = planJson.GetProperty("id").GetString();

                // 🔥 GUARDAMOS PARA LA VISTA
                TempData["PlanSeleccionado"] = plan;
                return RedirectToAction("MiMembresia", new
                {
                    plan = plan,
                    paypalPlanId = planId,
                    precio = precio          // ← NUEVO: incluye el precio en la ruta
                });
            }
            catch
            {
                TempData["Error"] = "Error al procesar el pago";
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Resultado(string subscription_id, string plan)
        {
            try
            {
                if (string.IsNullOrEmpty(subscription_id))
                {
                    ViewBag.Error = true;
                    ViewBag.Mensaje = "No se recibió confirmación de PayPal";
                    return View();
                }

                var accessToken = await GetAccessToken();
                var mode = _configuration["PayPal:Mode"];

                var baseUrl = mode == "sandbox"
                    ? "https://api-m.sandbox.paypal.com"
                    : "https://api-m.paypal.com";

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync(
                    $"{baseUrl}/v1/billing/subscriptions/{subscription_id}"
                );

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = true;
                    ViewBag.Mensaje = "No se pudo verificar la suscripción";
                    return View();
                }

                var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(
                    await response.Content.ReadAsStringAsync()
                );

                var estado = json.GetProperty("status").GetString();

                if (estado != "ACTIVE")
                {
                    ViewBag.Error = true;
                    ViewBag.Mensaje = $"Suscripción no activa (estado: {estado})";
                    return View();
                }

                var fechaFin = json
                    .GetProperty("billing_info")
                    .GetProperty("next_billing_time")
                    .GetDateTime();

                var usuario = await _userManager.GetUserAsync(User);

                usuario.TipoMembresia = plan;
                usuario.EstadoMembresia = "Activa";
                usuario.FechaInicioMembresia = DateTime.Now;
                usuario.FechaFinMembresia = fechaFin;
                usuario.PayPalSuscripcionId = subscription_id;

                await _userManager.UpdateAsync(usuario);

                ViewBag.Error = false;
                ViewBag.Plan = plan;
                ViewBag.FechaVencimiento = fechaFin;
                ViewBag.Mensaje = $"¡Suscripción activa hasta {fechaFin:dd/MM/yyyy}!";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Resultado");

                ViewBag.Error = true;
                ViewBag.Mensaje = "Error procesando la suscripción";

                return View();
            }
        }

    }
}

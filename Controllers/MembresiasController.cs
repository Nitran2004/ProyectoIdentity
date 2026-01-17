using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ProyectoIdentity.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ProyectoIdentity.Controllers
{
    public class MembresiasController : Controller
    {
        private readonly UserManager<AppUsuario> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MembresiasController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public MembresiasController(
            UserManager<AppUsuario> userManager,
            IConfiguration configuration,
            ILogger<MembresiasController> logger,
            IHttpClientFactory httpClientFactory)
        {
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

            var result = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(result);

            return json.GetProperty("access_token").GetString();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var usuario = await _userManager.GetUserAsync(User);
                ViewBag.MembresiaActual = usuario?.TipoMembresia;
                ViewBag.EstadoMembresia = usuario?.EstadoMembresia;
                ViewBag.FechaVencimiento = usuario?.FechaFinMembresia;
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SeleccionarPlan(string plan, decimal precio)
        {
            try
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null) return Unauthorized();

                _logger.LogInformation($"🔵 Usuario: {usuario.Email} seleccionó plan {plan}");

                var accessToken = await GetAccessToken();
                var mode = _configuration["PayPal:Mode"];
                var baseUrl = mode == "sandbox"
                    ? "https://api-m.sandbox.paypal.com"
                    : "https://api-m.paypal.com";

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // 1. Crear producto
                var producto = new
                {
                    name = $"Membresía El Nacional - {plan}",
                    type = "SERVICE"
                };

                var productoResponse = await client.PostAsync(
                    $"{baseUrl}/v1/catalogs/products",
                    new StringContent(JsonSerializer.Serialize(producto), Encoding.UTF8, "application/json")
                );

                var productoResult = await productoResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"Producto creado: {productoResult}");

                var productoData = JsonSerializer.Deserialize<JsonElement>(productoResult);
                var productoId = productoData.GetProperty("id").GetString();

                // 2. Crear plan de suscripción
                var planData = new
                {
                    product_id = productoId,
                    name = $"Plan {plan} - El Nacional",
                    description = $"Suscripción mensual {plan}",
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
                    }
                };

                var planResponse = await client.PostAsync(
                    $"{baseUrl}/v1/billing/plans",
                    new StringContent(JsonSerializer.Serialize(planData), Encoding.UTF8, "application/json")
                );

                var planResult = await planResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"Plan creado: {planResult}");

                var planJson = JsonSerializer.Deserialize<JsonElement>(planResult);
                var planId = planJson.GetProperty("id").GetString();

                // 3. Crear suscripción
                var startTime = DateTime.UtcNow.AddMinutes(5).ToString("yyyy-MM-ddTHH:mm:ssZ");

                var suscripcion = new
                {
                    plan_id = planId,
                    start_time = startTime,
                    subscriber = new
                    {
                        email_address = usuario.Email,
                        name = new
                        {
                            given_name = usuario.Nombre ?? "Usuario",
                            surname = "El Nacional"
                        }
                    },
                    application_context = new
                    {
                        brand_name = "Club El Nacional",
                        return_url = $"{_configuration["PayPal:SuccessUrl"]}?plan={plan}",
                        cancel_url = _configuration["PayPal:CancelUrl"]
                    }
                };

                var suscripcionResponse = await client.PostAsync(
                    $"{baseUrl}/v1/billing/subscriptions",
                    new StringContent(JsonSerializer.Serialize(suscripcion), Encoding.UTF8, "application/json")
                );

                var suscripcionResult = await suscripcionResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"Suscripción creada: {suscripcionResult}");

                var suscripcionJson = JsonSerializer.Deserialize<JsonElement>(suscripcionResult);

                var approveLink = suscripcionJson.GetProperty("links")
                    .EnumerateArray()
                    .FirstOrDefault(l => l.GetProperty("rel").GetString() == "approve")
                    .GetProperty("href").GetString();

                TempData["Plan"] = plan;
                TempData["Precio"] = precio.ToString("F2");

                return Redirect(approveLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error con PayPal");
                Console.WriteLine($"Error PayPal completo: {ex}");
                TempData["Error"] = $"Error: {ex.Message}";
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

                _logger.LogInformation($"✅ Suscripción confirmada: {subscription_id}");

                var usuario = await _userManager.GetUserAsync(User);

                usuario.TipoMembresia = plan;
                usuario.EstadoMembresia = "Activa";
                usuario.FechaInicioMembresia = DateTime.Now;
                usuario.FechaFinMembresia = DateTime.Now.AddMonths(1);
                usuario.PreapprovalId = subscription_id;

                await _userManager.UpdateAsync(usuario);

                ViewBag.Mensaje = $"¡Bienvenido al plan {plan}! Tu membresía está activa hasta el {usuario.FechaFinMembresia:dd/MM/yyyy}";
                ViewBag.Plan = plan;
                ViewBag.Error = false;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al procesar resultado");
                ViewBag.Error = true;
                ViewBag.Mensaje = "Error al procesar la suscripción";
                return View();
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MiMembresia()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario == null)
                return Unauthorized();

            if (usuario.FechaFinMembresia.HasValue &&
                usuario.FechaFinMembresia < DateTime.Now &&
                usuario.EstadoMembresia == "Activa")
            {
                usuario.EstadoMembresia = "Expirada";
                await _userManager.UpdateAsync(usuario);
            }

            return View(usuario);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CancelarMembresia()
        {
            try
            {
                var usuario = await _userManager.GetUserAsync(User);

                if (string.IsNullOrEmpty(usuario.PreapprovalId))
                {
                    TempData["Error"] = "No se encontró suscripción activa";
                    return RedirectToAction("MiMembresia");
                }

                var accessToken = await GetAccessToken();
                var mode = _configuration["PayPal:Mode"];
                var baseUrl = mode == "sandbox"
                    ? "https://api-m.sandbox.paypal.com"
                    : "https://api-m.paypal.com";

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var cancelData = new
                {
                    reason = "Usuario canceló la membresía"
                };

                var response = await client.PostAsync(
                    $"{baseUrl}/v1/billing/subscriptions/{usuario.PreapprovalId}/cancel",
                    new StringContent(JsonSerializer.Serialize(cancelData), Encoding.UTF8, "application/json")
                );

                if (response.IsSuccessStatusCode)
                {
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
    }
}
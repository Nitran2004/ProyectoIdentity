using Microsoft.AspNetCore.Mvc;

namespace ProyectoIdentity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MercadoPagoController : ControllerBase
    {
        // Endpoint para recibir el código de autorización
        [HttpGet("/callback")]
        public IActionResult Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("No se recibió código de autorización");
            }

            // Aquí guardarías el código en tu base de datos o lo procesarías
            Console.WriteLine($"✅ Código recibido: {code}");

            // Ahora debes intercambiar este código por un access token
            // (siguiente paso)

            return Ok("¡Autorización exitosa! Puedes cerrar esta ventana.");
        }

        // Endpoint para recibir notificaciones de pagos
        [HttpPost("/webhook")]
        public IActionResult Webhook([FromBody] dynamic notification)
        {
            Console.WriteLine("🔔 Notificación recibida:");
            Console.WriteLine(notification.ToString());

            // Aquí procesarías la notificación del pago
            // Por ejemplo: actualizar estado de suscripción en BD

            return Ok();
        }
    }
}
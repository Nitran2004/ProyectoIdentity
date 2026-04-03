using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoIdentity.Models
{
    public class WebhookPayPal
    {
        [Key]
        public int Id { get; set; }

        // ID único del evento enviado por PayPal
        [StringLength(100)]
        public string PayPalEventoId { get; set; }

        // Tipo de evento (PAYMENT.SALE.COMPLETED, BILLING.SUBSCRIPTION.CANCELLED, etc.)
        [StringLength(150)]
        public string TipoEvento { get; set; }

        // ID de la suscripción en PayPal
        [StringLength(100)]
        public string PayPalSuscripcionId { get; set; }

        // Estado recibido (ACTIVE, CANCELLED, SUSPENDED, etc.)
        [StringLength(50)]
        public string Estado { get; set; }

        // JSON completo enviado por PayPal (para auditoría)
        public string ContenidoJson { get; set; }

        // Fecha en que PayPal envió el webhook
        public DateTime FechaEvento { get; set; }

        // Fecha en que el sistema lo procesó
        public DateTime FechaRecepcion { get; set; }

        // Indica si el webhook ya fue procesado por el sistema
        public bool Procesado { get; set; }

        // Mensaje de error si falló el procesamiento
        [StringLength(300)]
        public string MensajeError { get; set; }
    }
}

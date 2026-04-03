using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIdentity.Models
{
    public class PagoSuscripcion
    {
        public int Id { get; set; }

        [Required]
        public int SuscripcionUsuarioId { get; set; }

        [ForeignKey(nameof(SuscripcionUsuarioId))]
        public SuscripcionUsuario Suscripcion { get; set; } = null!;

        [Required]
        public decimal Monto { get; set; }

        [Required, StringLength(3)]
        public string Moneda { get; set; } = "USD";

        [StringLength(30)]
        public string EstadoPago { get; set; } = "PENDIENTE";
        // PENDIENTE, COMPLETADO, FALLIDO, REEMBOLSADO

        [StringLength(120)]
        public string? PayPalTransaccionId { get; set; }

        [StringLength(80)]
        public string? PayPalEventoId { get; set; }

        public DateTime? FechaPago { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}

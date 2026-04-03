using System.ComponentModel.DataAnnotations;

namespace ProyectoIdentity.Models
{
    public class PlanMembresia
    {
        public int Id { get; set; }

        [Required, StringLength(60)]
        public string Nombre { get; set; } = null!; // Bronce, Plata, Oro, VIP

        [Required]
        public decimal Precio { get; set; }

        [Required, StringLength(3)]
        public string Moneda { get; set; } = "USD";

        // IDs de PayPal
        [StringLength(120)]
        public string? PayPalProductoId { get; set; }

        [StringLength(120)]
        public string? PayPalPlanId { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        public ICollection<SuscripcionUsuario> Suscripciones { get; set; } = new List<SuscripcionUsuario>();
    }
}

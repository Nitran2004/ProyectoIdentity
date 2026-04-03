using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIdentity.Models
{
    public class SuscripcionUsuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public AppUsuario Usuario { get; set; }

        [Required]
        public int PlanMembresiaId { get; set; }

        [ForeignKey(nameof(PlanMembresiaId))]
        public PlanMembresia Plan { get; set; }

        [StringLength(100)]
        public string PayPalSuscripcionId { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } // ACTIVA, CANCELADA, PAUSADA

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime? FechaCancelacion { get; set; }

        public bool EsActual { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
    }
}

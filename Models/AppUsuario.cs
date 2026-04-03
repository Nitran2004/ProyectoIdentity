using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIdentity.Models
{
    public class AppUsuario : IdentityUser
    {
        [StringLength(100)]
        public string? Nombre { get; set; }

        [StringLength(200)]
        public string? Cedula { get; set; }

        [StringLength(10)]
        public string? CodigoPais { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(50)]
        public string? Pais { get; set; }

        [StringLength(50)]
        public string? Ciudad { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(50)]
        public string? Estado { get; set; }

        [NotMapped]
        public string? Rol { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? ListaRoles { get; set; }

        [NotMapped]
        public string? IdRol { get; set; }

        [StringLength(50)]
        public string? TipoMembresia { get; set; }

        public DateTime? FechaInicioMembresia { get; set; }

        public DateTime? FechaFinMembresia { get; set; }

        [StringLength(20)]
        public string? EstadoMembresia { get; set; }

        [StringLength(100)]
        public string? PayPalSuscripcionId { get; set; }

        [StringLength(100)]
        public string? PayPalPlanId { get; set; }

    }
}

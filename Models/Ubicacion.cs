using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoIdentity.Models
{
    public class Ubicacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        [StringLength(100)]
        public string Ciudad { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(300)]
        public string Direccion { get; set; }

        [StringLength(20)]
        public string Celular { get; set; }

        [StringLength(300)]
        public string Horario { get; set; }

        [StringLength(500)]
        [Display(Name = "Link de ubicación")]
        public string Link { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;
    }
}
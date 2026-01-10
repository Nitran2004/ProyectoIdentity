using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProyectoIdentity.Models
{
    public class Escuela
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la escuela es obligatoria")]
        [Display(Name = "Nombre de la Escuela")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La region es obligatoria")]
        public string Region { get; set; }

        // Esto permite que una Escuela tenga muchas Sedes (botón más)
        public virtual ICollection<SedeEscuela> Sedes { get; set; } = new List<SedeEscuela>();
    }
}
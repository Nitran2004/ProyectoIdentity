using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoIdentity.Models
{
    public class SedeEscuela
    {
        [Key]
        public int Id { get; set; }

        // Campos SIN validación Required (Opcionales)
        public string Ciudad { get; set; }

        [Display(Name = "Coordinador")]
        public string Responsable { get; set; }

        public string Telefono { get; set; }

        [Display(Name = "Correo electronico")]
        public string EmailOrRedes { get; set; }

        public string Requisitos { get; set; }

        [Display(Name = "Link de Google Maps")]
        public string MapaLink { get; set; }

        // Campos CON validación Required (Obligatorios)
        [Required(ErrorMessage = "La dirección exacta es obligatoria")]
        public string Direccion { get; set; }

        [Display(Name = "Dias y Horarios de entrenamiento")]
        public string Horarios { get; set; }

        // Relación con la escuela
        public int EscuelaId { get; set; }
        [ForeignKey("EscuelaId")]
        public virtual Escuela Escuela { get; set; }
    }
}
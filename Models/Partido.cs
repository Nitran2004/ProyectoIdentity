using System.ComponentModel.DataAnnotations;

namespace ProyectoIdentity.Models
{
    public class Partido
    {
        public int Id { get; set; }

        public DateTime FechaHora { get; set; }

        [Required]
        public string Local { get; set; }

        [Required]
        public string Visitante { get; set; }

        public string Estadio { get; set; }

        public byte[]? ImagenLocal { get; set; }

        public byte[]? ImagenVisitante { get; set; }

        public string? EntradasLink { get; set; }
    }
}

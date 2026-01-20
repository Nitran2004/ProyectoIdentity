using System.ComponentModel.DataAnnotations;

namespace ProyectoIdentity.Models
{
    public class TablaPosicion
    {
        public int Id { get; set; }

        public int Posicion { get; set; }

        public string Club { get; set; }

        public byte[]? ImagenClub { get; set; }

        public int Puntos { get; set; }
    }

}

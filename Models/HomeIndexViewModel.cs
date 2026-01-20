using System.Collections.Generic;

namespace ProyectoIdentity.Models
{
    public class HomeIndexViewModel
    {
        public Partido? ProximoPartido { get; set; }
        public List<TablaPosicion> Tabla { get; set; } = new();
    }
}
namespace ProyectoIdentity.Models
{
    public class HomeViewModel
    {
        public List<TablaPosicion> Tabla { get; set; } = new();
        public Partido? ProximoPartido { get; set; }
    }
}

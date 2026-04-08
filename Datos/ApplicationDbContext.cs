using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProyectoIdentity.Models;

namespace ProyectoIdentity.Datos
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        // ✅ DBSETS
        public DbSet<AppUsuario> AppUsuario { get; set; }
        public DbSet<SuscripcionUsuario> SuscripcionesUsuario { get; set; }
        public DbSet<PlanMembresia> PlanesMembresia { get; set; }
        public DbSet<PagoSuscripcion> PagosSuscripcion { get; set; }
        public DbSet<WebhookPayPal> WebhooksPayPal { get; set; }
        public DbSet<TablaPosicion> TablaPosiciones { get; set; }
        public DbSet<Partido> Partidos { get; set; }
        public DbSet<Escuela> Escuelas { get; set; }
        public DbSet<Ubicacion> Ubicaciones { get; set; }


        public DbSet<SedeEscuela> SedesEscuelas { get; set; } // <--- ¡Añade esto!
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ CONFIGURACIÓN PARA APPUSUARIO
            modelBuilder.Entity<AppUsuario>(entity =>
            {
                entity.Property(e => e.Nombre).HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Pais).HasMaxLength(50);
                entity.Property(e => e.Ciudad).HasMaxLength(50);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.CodigoPais).HasMaxLength(10);
                entity.Property(e => e.Estado).HasMaxLength(50);
                entity.Property(e => e.Cedula).HasMaxLength(200);

            });

        }
    }
}
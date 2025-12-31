using Microsoft.EntityFrameworkCore;
using BaseConocimiento.Domain.Entities;
using BaseConocimiento.Infrastructure.Data.Configurations;

namespace BaseConocimiento.Infrastructure.Data
{
    public class BaseConocimientoDbContext : DbContext
    {
        public BaseConocimientoDbContext(DbContextOptions<BaseConocimientoDbContext> options)
              : base(options) { }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Manual> Manuales { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<ConsultaManual> ConsultasManuales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
            modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
            modelBuilder.ApplyConfiguration(new ManualConfiguration());
            modelBuilder.ApplyConfiguration(new ConsultaConfiguration());
            modelBuilder.ApplyConfiguration(new ConsultaManualConfiguration());
        }
    }

}


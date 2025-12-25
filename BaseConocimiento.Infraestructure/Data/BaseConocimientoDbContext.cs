using Microsoft.EntityFrameworkCore;
using BaseConocimiento.Domain.Entities;

namespace BaseConocimiento.Infrastructure.Data
{
    public class BaseConocimientoDbContext : DbContext
    {
        public BaseConocimientoDbContext(DbContextOptions<BaseConocimientoDbContext> options)
            : base(options)
        {
        }

        public DbSet<Manual> Manuales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Manual>(entity =>
            {
                entity.ToTable("manuales");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .IsRequired();

                entity.Property(e => e.Titulo)
                    .HasColumnName("titulo")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Categoria)
                    .HasColumnName("categoria")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.SubCategoria)
                    .HasColumnName("sub_categoria")
                    .HasMaxLength(100);

                entity.Property(e => e.Version)
                    .HasColumnName("version")
                    .HasMaxLength(50);

                entity.Property(e => e.Descripcion)
                    .HasColumnName("descripcion")
                    .HasColumnType("text");

                entity.Property(e => e.RutaLocal)
                    .HasColumnName("ruta_local")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.NombreOriginal)
                    .HasColumnName("nombre_original")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.UsuarioId)
                    .HasColumnName("usuario_id")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.FechaSubida)
                    .HasColumnName("fecha_subida")
                    .IsRequired();

                entity.Property(e => e.PesoArchivo)
                    .HasColumnName("peso_archivo")
                    .IsRequired();

                entity.Property(e => e.Estado)
                    .HasColumnName("estado")
                    .HasConversion<int>()
                    .IsRequired();

                // Índices
                entity.HasIndex(e => e.Categoria)
                    .HasDatabaseName("idx_manuales_categoria");

                entity.HasIndex(e => new { e.Categoria, e.SubCategoria })
                    .HasDatabaseName("idx_manuales_categoria_subcategoria");

                entity.HasIndex(e => e.Estado)
                    .HasDatabaseName("idx_manuales_estado");

                entity.HasIndex(e => e.FechaSubida)
                    .HasDatabaseName("idx_manuales_fecha_subida");

                entity.HasIndex(e => e.UsuarioId)
                    .HasDatabaseName("idx_manuales_usuario_id");
            });
        }
    }
}

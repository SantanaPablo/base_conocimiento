using BaseConocimiento.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Infrastructure.Data.Configurations
{
    public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            builder.ToTable("categorias");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("id");

            builder.Property(c => c.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Descripcion)
                .HasColumnName("descripcion");

            builder.Property(c => c.CategoriaPadreId)
                .HasColumnName("categoria_padre_id");

            builder.Property(c => c.Color)
                .HasColumnName("color")
                .HasMaxLength(20);

            builder.Property(c => c.Icono)
                .HasColumnName("icono")
                .HasMaxLength(50);

            builder.Property(c => c.Orden)
                .HasColumnName("orden");

            builder.Property(c => c.EsActiva)
                .HasColumnName("es_activa");

            builder.Property(c => c.FechaCreacion)
                .HasColumnName("fecha_creacion");

            // Relaciones
            builder.HasOne(c => c.CategoriaPadre)
                .WithMany(c => c.SubCategorias)
                .HasForeignKey(c => c.CategoriaPadreId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Manuales)
                .WithOne(m => m.Categoria)
                .HasForeignKey(m => m.CategoriaId);

            builder.HasIndex(c => c.Nombre).IsUnique();
        }
    }
}

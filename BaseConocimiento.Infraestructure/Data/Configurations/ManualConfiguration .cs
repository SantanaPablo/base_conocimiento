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
    public class ManualConfiguration : IEntityTypeConfiguration<Manual>
    {
        public void Configure(EntityTypeBuilder<Manual> builder)
        {
            builder.ToTable("manuales");

            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).HasColumnName("id");

            builder.Property(m => m.Titulo)
                .HasColumnName("titulo")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(m => m.CategoriaId)
                .HasColumnName("categoria_id")
                .IsRequired();

            builder.Property(m => m.SubCategoria)
                .HasColumnName("sub_categoria")
                .HasMaxLength(100);

            builder.Property(m => m.Version)
                .HasColumnName("version")
                .HasMaxLength(50);

            builder.Property(m => m.Descripcion)
                .HasColumnName("descripcion");

            builder.Property(m => m.RutaStorage)
                .HasColumnName("ruta_storage")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(m => m.NombreOriginal)
                .HasColumnName("nombre_original")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(m => m.UsuarioId)
                .HasColumnName("usuario_id")
                .IsRequired();

            builder.Property(m => m.FechaSubida)
                .HasColumnName("fecha_subida");

            builder.Property(m => m.PesoArchivo)
                .HasColumnName("peso_archivo");

            builder.Property(m => m.Estado)
                .HasColumnName("estado")
                .HasConversion<int>();

            builder.Property(m => m.NumeroConsultas)
                .HasColumnName("numero_consultas");

            builder.Property(m => m.UltimaConsulta)
                .HasColumnName("ultima_consulta");

            // Relaciones
            builder.HasOne(m => m.Categoria)
                .WithMany(c => c.Manuales)
                .HasForeignKey(m => m.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Usuario)
                .WithMany(u => u.ManualesSubidos)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(m => m.CategoriaId);
            builder.HasIndex(m => m.UsuarioId);
            builder.HasIndex(m => m.Estado);
        }
    }
}

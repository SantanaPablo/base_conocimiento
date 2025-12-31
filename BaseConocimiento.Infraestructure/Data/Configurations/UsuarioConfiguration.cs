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
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("usuarios");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).HasColumnName("id");

            builder.Property(u => u.NombreCompleto)
                .HasColumnName("nombre_completo")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500) 
            .IsRequired();

            builder.Property(u => u.Departamento)
                .HasColumnName("departamento")
                .HasMaxLength(100);

            builder.Property(u => u.Rol)
                .HasColumnName("rol")
                .HasConversion<int>();

            builder.Property(u => u.EsActivo)
                .HasColumnName("es_activo");

            builder.Property(u => u.FechaRegistro)
                .HasColumnName("fecha_registro");

            builder.Property(u => u.UltimoAcceso)
                .HasColumnName("ultimo_acceso");

            // Relaciones
            builder.HasMany(u => u.ManualesSubidos)
                .WithOne(m => m.Usuario)
                .HasForeignKey(m => m.UsuarioId);

            builder.HasMany(u => u.ConsultasRealizadas)
                .WithOne(c => c.Usuario)
                .HasForeignKey(c => c.UsuarioId);

            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
}

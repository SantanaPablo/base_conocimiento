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
    public class ConsultaConfiguration : IEntityTypeConfiguration<Consulta>
    {
        public void Configure(EntityTypeBuilder<Consulta> builder)
        {
            builder.ToTable("consultas");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("id");

            builder.Property(c => c.Pregunta)
                .HasColumnName("pregunta")
                .IsRequired();

            builder.Property(c => c.Respuesta)
                .HasColumnName("respuesta")
                .IsRequired();

            builder.Property(c => c.ConversacionId)
                .HasColumnName("conversacion_id")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.UsuarioId)
                .HasColumnName("usuario_id")
                .IsRequired();

            builder.Property(c => c.ResultadosEncontrados)
                .HasColumnName("resultados_encontrados");

            builder.Property(c => c.TiempoRespuestaMs)
                .HasColumnName("tiempo_respuesta_ms");

            builder.Property(c => c.FechaConsulta)
                .HasColumnName("fecha_consulta");

            builder.Property(c => c.FueUtil)
                .HasColumnName("fue_util");

            // Relaciones
            builder.HasOne(c => c.Usuario)
                .WithMany(u => u.ConsultasRealizadas)
                .HasForeignKey(c => c.UsuarioId);

            builder.HasIndex(c => c.ConversacionId);
        }
    }
}

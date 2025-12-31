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
    public class ConsultaManualConfiguration : IEntityTypeConfiguration<ConsultaManual>
    {
        public void Configure(EntityTypeBuilder<ConsultaManual> builder)
        {
            builder.ToTable("consultas_manuales");

            builder.HasKey(cm => new { cm.ConsultaId, cm.ManualId });

            builder.Property(cm => cm.ConsultaId)
                .HasColumnName("consulta_id");

            builder.Property(cm => cm.ManualId)
                .HasColumnName("manual_id");

            builder.Property(cm => cm.RelevanciaPromedio)
                .HasColumnName("relevancia_promedio");

            // Relaciones
            builder.HasOne(cm => cm.Consulta)
                .WithMany(c => c.ManualesConsultados)
                .HasForeignKey(cm => cm.ConsultaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cm => cm.Manual)
                .WithMany(m => m.ConsultasRelacionadas)
                .HasForeignKey(cm => cm.ManualId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

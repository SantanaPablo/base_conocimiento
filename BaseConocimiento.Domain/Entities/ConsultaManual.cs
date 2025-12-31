using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Domain.Entities
{
    public class ConsultaManual
    {
        public Guid ConsultaId { get; set; }
        public Guid ManualId { get; set; }
        public double RelevanciaPromedio { get; set; }

        // Navegación
        public Consulta Consulta { get; set; }
        public Manual Manual { get; set; }
    }
}

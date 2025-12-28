using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Domain.Entities
{
    public class ConversacionMetadata
    {
        public string ConversacionId { get; set; }
        public string UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime UltimaActividad { get; set; }
        public int TotalMensajes { get; set; }
    }
}

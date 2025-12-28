using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Domain.Entities
{
    public class MensajeConversacion
    {
        public string Rol { get; set; }
        public string Contenido { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

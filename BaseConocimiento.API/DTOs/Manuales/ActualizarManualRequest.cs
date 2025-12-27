using BaseConocimiento.Domain.Enums;

namespace BaseConocimiento.API.DTOs.Manuales
{
    public class ActualizarManualRequest
    {
        public string Version { get; set; }
        public string Descripcion { get; set; }
    }
}

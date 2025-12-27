using BaseConocimiento.Domain.Enums;

namespace BaseConocimiento.API.DTOs.Manuales
{
    public class ActualizarEstadoRequest
    {
        public EstadoManual NuevoEstado { get; set; }
    }
}

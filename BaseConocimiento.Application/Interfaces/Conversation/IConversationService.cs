using BaseConocimiento.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.Interfaces.Conversation
{
    public interface IConversationService
    {
        // Crear nueva conversación
        Task<string> CrearConversacionAsync(string usuarioId);

        // Agregar mensaje a conversación
        Task AgregarMensajeAsync(string conversacionId, string rol, string contenido);

        // Obtener historial completo
        Task<List<MensajeConversacion>> ObtenerHistorialAsync(string conversacionId);

        // Obtener últimos N mensajes
        Task<List<MensajeConversacion>> ObtenerUltimosMensajesAsync(string conversacionId, int cantidad = 10);

        // Limpiar conversación
        Task LimpiarConversacionAsync(string conversacionId);

        // Verificar si existe
        Task<bool> ExisteConversacionAsync(string conversacionId);

        // Obtener metadata
        Task<ConversacionMetadata> ObtenerMetadataAsync(string conversacionId);
    }
}

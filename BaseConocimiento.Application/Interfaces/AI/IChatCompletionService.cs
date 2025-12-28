using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseConocimiento.Domain.Entities;

namespace BaseConocimiento.Application.Interfaces.AI
{
    public interface IChatCompletionService
    {
        Task<string> GenerarRespuestaAsync(string prompt);

        Task<string> GenerarRespuestaConHistorialAsync(
            string prompt,
            List<MensajeConversacion> historial);
    }
}

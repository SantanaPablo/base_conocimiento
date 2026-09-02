using BaseConocimiento.Domain.Entities;

namespace BaseConocimiento.Application.Interfaces.AI
{
    public interface IChatCompletionService
    {
        Task<string> GenerarRespuestaAsync(string prompt, CancellationToken ct = default);

        Task<string> GenerarRespuestaConHistorialAsync(
            string? systemPrompt,
            string userPrompt,
            List<MensajeConversacion> historial,
            CancellationToken ct = default);
    }
}
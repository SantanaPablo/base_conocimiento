// Infrastructure/Services/AI/Ollama/OllamaChatService.cs
using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace BaseConocimiento.Infrastructure.Services.AI.Ollama
{
    public class OllamaChatService : IChatCompletionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaChatService> _logger;
        private const string MODEL = "inuzaru";

        public OllamaChatService(HttpClient httpClient, ILogger<OllamaChatService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.BaseAddress = new Uri("http://localhost:11434/api/");
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<string> GenerarRespuestaAsync(string prompt)
        {
            return await GenerarRespuestaConHistorialAsync(prompt, new List<MensajeConversacion>());
        }

        // ✅ CORREGIDO: Usar /api/chat en lugar de /api/generate
        public async Task<string> GenerarRespuestaConHistorialAsync(
            string prompt,
            List<MensajeConversacion> historial)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                // ✅ Construir mensajes en formato Ollama
                var messages = new List<object>();

                // Agregar historial
                foreach (var mensaje in historial.TakeLast(5))
                {
                    messages.Add(new
                    {
                        role = mensaje.Rol, // "user" o "assistant"
                        content = mensaje.Contenido
                    });
                }

                // Agregar mensaje actual
                messages.Add(new
                {
                    role = "user",
                    content = prompt
                });

                // ✅ IMPORTANTE: Usar /api/chat para que respete el SYSTEM del Modelfile
                var request = new
                {
                    model = MODEL,
                    messages = messages,
                    stream = false,
                    options = new
                    {
                        temperature = 0.3,  // Igual que en Modelfile
                        num_ctx = 4096,     // Igual que en Modelfile
                        num_predict = 512,
                        num_gpu = 1,
                        num_thread = 8
                    }
                };

                _logger.LogInformation("🤖 Generando respuesta con {Count} mensajes de historial...",
                    historial.Count);

                // ✅ CAMBIO CLAVE: Usar /api/chat en lugar de /api/generate
                var response = await _httpClient.PostAsJsonAsync("chat", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Ollama error {Status}: {Content}", response.StatusCode, errorContent);
                    return "Error: Ollama no está respondiendo correctamente.";
                }

                var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

                sw.Stop();
                _logger.LogInformation("✅ Respuesta generada en {Ms}ms", sw.ElapsedMilliseconds);

                return result?.message?.content ?? "Sin respuesta del modelo.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generando respuesta");
                return $"Error: {ex.Message}";
            }
        }

      
    }
}
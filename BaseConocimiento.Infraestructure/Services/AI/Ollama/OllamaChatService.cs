// Infrastructure/Services/AI/Ollama/OllamaChatService.cs
using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Json;

namespace BaseConocimiento.Infrastructure.Services.AI.Ollama
{
    public class OllamaChatService : IChatCompletionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaChatService> _logger;
        private readonly string _modelName;
        private readonly double _temperature;
        private readonly int _numCtx;
        private readonly int _numGpu;
        private readonly int _numPredict;
        private readonly int _numThread;

        public OllamaChatService(HttpClient httpClient, ILogger<OllamaChatService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;

            _modelName = configuration["AI:ModelName"] ?? "llama3.2:3b";
            _temperature = configuration.GetValue("AI:Temperature", 0.3);
            _numCtx = configuration.GetValue("AI:NumCtx", 4096);
            _numGpu = configuration.GetValue("AI:NumGpu", 21);
            _numPredict = configuration.GetValue("AI:NumPredict", 512);
            _numThread = configuration.GetValue("AI:NumThread", 8);

            _httpClient.Timeout = TimeSpan.FromSeconds(100);
        }

        public async Task<string> GenerarRespuestaAsync(string prompt, CancellationToken ct = default)
        {
            return await GenerarRespuestaConHistorialAsync(
                systemPrompt: null,
                userPrompt: prompt,
                historial: new List<MensajeConversacion>(),
                ct: ct);
        }

        public async Task<string> GenerarRespuestaConHistorialAsync(
            string? systemPrompt,
            string userPrompt,
            List<MensajeConversacion> historial,
            CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var messages = new List<object>();

                // El system, si viene, se manda SIEMPRE primero.
                // Ojo: esto reemplaza al SYSTEM del Modelfile para esta llamada puntual, no se combinan.
                if (!string.IsNullOrWhiteSpace(systemPrompt))
                {
                    messages.Add(new { role = "system", content = systemPrompt });
                }

                foreach (var mensaje in historial.TakeLast(5))
                {
                    messages.Add(new
                    {
                        role = mensaje.Rol, // "user" o "assistant"
                        content = mensaje.Contenido
                    });
                }

                messages.Add(new
                {
                    role = "user",
                    content = userPrompt
                });

                var request = new
                {
                    model = _modelName,
                    messages,
                    stream = false,
                    options = new
                    {
                        temperature = _temperature,
                        num_ctx = _numCtx,
                        num_predict = _numPredict,
                        num_gpu = _numGpu,
                        num_thread = _numThread
                    }
                };

                _logger.LogInformation(
                    "Generando respuesta [modelo={Modelo}] con {Count} mensajes de historial (system={TieneSystem})",
                    _modelName, historial.Count, !string.IsNullOrWhiteSpace(systemPrompt));

                var response = await _httpClient.PostAsJsonAsync("api/chat", request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Ollama error {Status}: {Content}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Ollama no pudo procesar la solicitud: {response.StatusCode}");
                }

                var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: ct);

                sw.Stop();
                _logger.LogInformation("Respuesta generada en {Ms}ms", sw.ElapsedMilliseconds);

                return result?.message?.content ?? "Sin respuesta del modelo.";
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Consulta a Ollama cancelada (timeout o cancelación del cliente)");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico en el servicio de Chat Ollama");
                throw;
            }
        }
    }
}
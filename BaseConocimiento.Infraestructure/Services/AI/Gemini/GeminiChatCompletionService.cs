using BaseConocimiento.Application.Interfaces.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.Retry;
using BaseConocimiento.Domain.Entities;

namespace BaseConocimiento.Infrastructure.Services.AI.Gemini
{
    public class GeminiChatCompletionService : IChatCompletionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GeminiChatCompletionService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        private const string MODEL = "gemini-2.0-flash-lite";

        public GeminiChatCompletionService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiChatCompletionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _apiKey = configuration["AI:ApiKey"];

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogCritical("La API Key de Gemini no está configurada en appsettings.json");
            }

            _retryPolicy = Policy
                .Handle<HttpRequestException>(ex =>
                    ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Chat reintento {RetryCount} debido a error de red/servidor. Esperando {Delay}s",
                            retryCount,
                            timeSpan.TotalSeconds
                        );
                    }
                );

            _logger.LogInformation("Gemini Chat Service inicializado con el modelo {Model}", MODEL);
        }

        public async Task<string> GenerarRespuestaAsync(string prompt)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[] { new { text = prompt } }
                            }
                        }
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var url = $"https://generativelanguage.googleapis.com/v1/models/{MODEL}:generateContent?key={_apiKey}";
                    _logger.LogInformation("URL Final de llamada: {Url}", url);
                    var response = await _httpClient.PostAsync(url, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Google respondió con error {Code}: {Body}", response.StatusCode, errorBody);
                        response.EnsureSuccessStatusCode();
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogWarning("Rate limit alcanzado en Gemini API (Chat)");
                        throw new HttpRequestException("Rate limit exceeded", null, System.Net.HttpStatusCode.TooManyRequests);
                    }

                    

                  
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();

       
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var geminiResponse = JsonSerializer.Deserialize<GeminiChatResponse>(responseBody, options);

                    var respuestaTexto = geminiResponse?.candidates?[0]?.content?.parts?[0]?.text
                        ?? "No se pudo generar una respuesta.";

                    _logger.LogInformation("Respuesta Gemini recibida con éxito. Longitud: {Length} caracteres", respuestaTexto.Length);

                    return respuestaTexto;
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogError("Error 404: No se encontró el modelo o el endpoint de Gemini es inválido. Revisa la API Key y el nombre del modelo.");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fatal al comunicarse con Gemini Chat");
                    throw;
                }
            });
        }

        public Task<string> GenerarRespuestaConHistorialAsync(string prompt, List<MensajeConversacion> historial)
        {
            throw new NotImplementedException();
        }

        private class GeminiChatResponse
        {
            public Candidate[] candidates { get; set; }
        }

        private class Candidate
        {
            public Content content { get; set; }
        }

        private class Content
        {
            public Part[] parts { get; set; }
        }

        private class Part
        {
            public string text { get; set; }
        }
    }
}
using BaseConocimiento.Application.Interfaces.AI;
using System.Net.Http.Json;

namespace BaseConocimiento.Infrastructure.Services.AI.Ollama
{
    public class OllamaChatService : IChatCompletionService
    {
        private readonly HttpClient _httpClient;
        private const string MODEL = "agente-manuales";

        public OllamaChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:11434/api/");
        }

        public async Task<string> GenerarRespuestaAsync(string prompt)
        {
            var request = new
            {
                model = MODEL,
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = 0.2,
                    num_gpu = 1
                }
            };

            var response = await _httpClient.PostAsJsonAsync("generate", request);

            if (!response.IsSuccessStatusCode)
                return "Error: Ollama no está respondiendo. Verifica que esté abierto.";

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
            return result?.response ?? "Sin respuesta del modelo local.";
        }
    }
}
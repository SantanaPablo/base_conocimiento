using BaseConocimiento.Application.Interfaces.AI;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace BaseConocimiento.Infrastructure.Services.AI.Ollama
{
    public class OllamaEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _modelName;

        public OllamaEmbeddingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(100);
            _modelName = configuration["AI:EmbeddingModelName"] ?? "nomic-embed-text";
        }

        public async Task<float[]> GenerarEmbeddingAsync(string texto)
        {
            var request = new { model = _modelName, prompt = texto };

            var response = await _httpClient.PostAsJsonAsync("api/embeddings", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

            return result?.embedding?.Select(e => (float)e).ToArray() ?? Array.Empty<float>();
        }

        public async Task<List<float[]>> GenerarEmbeddingsAsync(List<string> textos)
        {
            var resultados = new List<float[]>();

            foreach (var texto in textos)
            {
                var embedding = await GenerarEmbeddingAsync(texto);
                resultados.Add(embedding);
            }

            return resultados;
        }
    }
}
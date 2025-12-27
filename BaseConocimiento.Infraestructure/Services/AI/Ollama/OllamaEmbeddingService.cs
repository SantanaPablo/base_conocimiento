using BaseConocimiento.Application.Interfaces.AI;
using System.Net.Http.Json;

namespace BaseConocimiento.Infrastructure.Services.AI.Ollama
{
    public class OllamaEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private const string MODEL = "nomic-embed-text";

        public OllamaEmbeddingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:11434/api/");
        }

        public async Task<float[]> GenerarEmbeddingAsync(string texto)
        {
            var request = new { model = MODEL, prompt = texto };

            var response = await _httpClient.PostAsJsonAsync("embeddings", request);
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
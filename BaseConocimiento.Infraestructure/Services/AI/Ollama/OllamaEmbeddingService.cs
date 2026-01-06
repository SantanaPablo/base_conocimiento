using BaseConocimiento.Application.Interfaces.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BaseConocimiento.Infrastructure.Services.AI.Ollama
{
    public class OllamaEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _modelName;
        private readonly ILogger<OllamaEmbeddingService> _logger;

        public OllamaEmbeddingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OllamaEmbeddingService> logger)
        {
            _logger = logger;
            _httpClient = httpClient;

            _httpClient.Timeout = TimeSpan.FromSeconds(150);
            _modelName = configuration["AI:EmbeddingModelName"] ?? "bge-m3";

            _logger.LogInformation("Servicio de Embeddings inicializado con el modelo: {Model}", _modelName);
        }

        public async Task<float[]> GenerarEmbeddingAsync(string texto)
        {
            try
            {
                // Limpieza básica para evitar vectores ruidosos
                if (string.IsNullOrWhiteSpace(texto)) return Array.Empty<float>();

                var request = new { model = _modelName, prompt = texto.Trim() };

                var response = await _httpClient.PostAsJsonAsync("api/embeddings", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

                if (result?.embedding == null)
                {
                    _logger.LogWarning("Ollama devolvió un embedding nulo para el texto proporcionado.");
                    return Array.Empty<float>();
                }

                // Convertimos double[] a float[] para compatibilidad con Qdrant
                return result.embedding.Select(e => (float)e).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar embedding individual con Ollama");
                throw;
            }
        }

        public async Task<List<float[]>> GenerarEmbeddingsAsync(List<string> textos)
        {
            if (textos == null || !textos.Any()) return new List<float[]>();

            _logger.LogInformation("Generando embeddings para un lote de {Count} fragmentos...", textos.Count);

            try
            {
                var tareas = textos.Select(t => GenerarEmbeddingAsync(t));

                var resultados = await Task.WhenAll(tareas);

                return resultados.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en el procesamiento por lotes de embeddings");
                throw;
            }
        }
    }
}
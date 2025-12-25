using BaseConocimiento.Application.Interfaces.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.Retry;

namespace BaseConocimiento.Infrastructure.Services.AI.Gemini
{
    public class GeminiEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GeminiEmbeddingService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly SemaphoreSlim _rateLimiter;

        private const string EMBEDDING_MODEL = "text-embedding-004";
        private const int MAX_CONCURRENT_REQUESTS = 2; // Máximo 2 requests simultáneos
        private const int DELAY_BETWEEN_REQUESTS_MS = 1000; // 1 segundo entre requests

        public GeminiEmbeddingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiEmbeddingService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["AI:ApiKey"];
            _logger = logger;

            _rateLimiter = new SemaphoreSlim(MAX_CONCURRENT_REQUESTS);

            _retryPolicy = Policy
                .Handle<HttpRequestException>(ex =>
                    ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Reintento {RetryCount} después de {Delay}s debido a: {Error}",
                            retryCount,
                            timeSpan.TotalSeconds,
                            exception.Message
                        );
                    }
                );

            _logger.LogInformation("Gemini Embedding Service inicializado con rate limiting");
        }

        public async Task<float[]> GenerarEmbeddingAsync(string texto)
        {
            await _rateLimiter.WaitAsync();

            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        var requestBody = new
                        {
                            model = $"models/{EMBEDDING_MODEL}",
                            content = new
                            {
                                parts = new[] { new { text = texto } }
                            }
                        };

                        var json = JsonSerializer.Serialize(requestBody);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await _httpClient.PostAsync(
                            $"https://generativelanguage.googleapis.com/v1beta/models/{EMBEDDING_MODEL}:embedContent?key={_apiKey}",
                            content
                        );

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            _logger.LogWarning("Rate limit alcanzado, esperando antes de reintentar...");
                            throw new HttpRequestException("Rate limit exceeded", null, System.Net.HttpStatusCode.TooManyRequests);
                        }

                        response.EnsureSuccessStatusCode();

                        var responseBody = await response.Content.ReadAsStringAsync();
                        var geminiResponse = JsonSerializer.Deserialize<GeminiEmbeddingResponse>(responseBody);

                        var embedding = geminiResponse?.embedding?.values ?? Array.Empty<float>();

                        _logger.LogDebug("Embedding generado: {Dimensions} dimensiones", embedding.Length);

                        await Task.Delay(DELAY_BETWEEN_REQUESTS_MS);

                        return embedding;
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al generar embedding con Gemini");
                        throw;
                    }
                });
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        public async Task<List<float[]>> GenerarEmbeddingsAsync(List<string> textos)
        {
            var embeddings = new List<float[]>();

            _logger.LogInformation("Generando {Count} embeddings con rate limiting", textos.Count);

            // Procesar de forma secuencial para evitar rate limits
            foreach (var texto in textos)
            {
                var embedding = await GenerarEmbeddingAsync(texto);
                embeddings.Add(embedding);
            }

            _logger.LogInformation("Completados {Count} embeddings", embeddings.Count);

            return embeddings;
        }

        private class GeminiEmbeddingResponse
        {
            public EmbeddingData embedding { get; set; }
        }

        private class EmbeddingData
        {
            public float[] values { get; set; }
        }
    }
}
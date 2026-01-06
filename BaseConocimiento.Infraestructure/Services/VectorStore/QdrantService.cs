using BaseConocimiento.Application.Interfaces.VectorStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace BaseConocimiento.Infrastructure.Services.VectorStore
{
    public class QdrantService : IQdrantService
    {
        private readonly QdrantClient _client;
        private readonly string _collectionName;
        private readonly ulong _vectorSize;
        private readonly ILogger<QdrantService> _logger;

        public QdrantService(IConfiguration configuration, ILogger<QdrantService> logger)
        {
            _logger = logger;

            var host = configuration["Qdrant:Host"] ?? "localhost";
            var port = int.Parse(configuration["Qdrant:Port"] ?? "6334");
            _collectionName = configuration["Qdrant:CollectionName"] ?? "base_conocimiento";

            //Tamaño dinámico
            _vectorSize = ulong.Parse(configuration["Qdrant:VectorSize"] ?? "1024");

            _client = new QdrantClient(host, port);

            _logger.LogInformation("Inuzaru-Qdrant conectado: {Host}:{Port}, Colección: {Collection}, Dimensiones: {Size}",
                host, port, _collectionName, _vectorSize);

            // Inicialización segura
            InicializarColeccionAsync().GetAwaiter().GetResult();
        }

        private async Task InicializarColeccionAsync()
        {
            try
            {
                //Verificación de existencia
                var colecciones = await _client.ListCollectionsAsync();
                bool existe = colecciones.Contains(_collectionName);

                if (!existe)
                {
                    _logger.LogInformation("🚀 Creando nueva colección: {CollectionName}...", _collectionName);

                    await _client.CreateCollectionAsync(
                        _collectionName,
                        new VectorParams
                        {
                            Size = _vectorSize,
                            Distance = Distance.Cosine // bge-m3 recomienda Cosine
                        }
                    );

                    _logger.LogInformation("✅ Colección Qdrant creada exitosamente.");
                }
                else
                {
                    _logger.LogInformation("♻️ Usando colección existente: {CollectionName}", _collectionName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error crítico al inicializar Qdrant. Verifique que VectorSize coincida con la colección existente.");
                throw;
            }
        }

        public async Task AlmacenarVectoresAsync(Guid manualId, List<VectorChunk> chunks)
        {
            try
            {
                if (chunks == null || !chunks.Any()) return;

                var points = new List<PointStruct>();

                foreach (var chunk in chunks)
                {
                    var payload = new Dictionary<string, Value>
                    {
                        { "manual_id", manualId.ToString() },
                        { "texto_original", chunk.TextoOriginal },
                        { "numero_pagina", chunk.NumeroPagina },
                        { "numero_chunk", chunk.NumeroChunk },
                        { "categoria_id", chunk.Categoria ?? "" },
                        { "titulo", chunk.Titulo ?? "" }
                    };

                    points.Add(new PointStruct
                    {
                        Id = Guid.NewGuid(),
                        Vectors = chunk.Vector, // Debe ser float[1024]
                        Payload = { payload }
                    });
                }

                await _client.UpsertAsync(_collectionName, points);
                _logger.LogInformation("✅ {Count} vectores indexados para el manual {Id}", chunks.Count, manualId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al insertar vectores en Qdrant");
                throw;
            }
        }

        public async Task<List<ResultadoBusqueda>> BuscarSimilaresAsync(
            float[] embedding,
            int topK = 5,
            string? categoriaId = null)
        {
            try
            {
                Filter? filter = null;

                if (!string.IsNullOrEmpty(categoriaId))
                {
                    filter = new Filter
                    {
                        Must = { new Condition { Field = new FieldCondition { Key = "categoria_id", Match = new Match { Keyword = categoriaId } } } }
                    };
                }

                float thresholdBge = 0.45f;

                var searchResult = await _client.SearchAsync(
                    _collectionName,
                    embedding,
                    filter: filter,
                    limit: (ulong)(topK * 2), // Traemos extra para filtrar basura corta
                    scoreThreshold: thresholdBge
                );

                var resultados = searchResult
                    .Select(r => new ResultadoBusqueda
                    {
                        ManualId = Guid.Parse(r.Payload["manual_id"].StringValue),
                        Titulo = r.Payload.ContainsKey("titulo") ? r.Payload["titulo"].StringValue : "Sin título",
                        TextoOriginal = r.Payload["texto_original"].StringValue,
                        NumeroPagina = (int)r.Payload["numero_pagina"].IntegerValue,
                        Score = r.Score
                    })
                    .Where(r => r.TextoOriginal.Length >= 50)
                    .OrderByDescending(r => r.Score)
                    .Take(topK)
                    .ToList();

                _logger.LogInformation("🔍 Búsqueda realizada: {Count} candidatos encontrados sobre el umbral {T}",
                    resultados.Count, thresholdBge);

                return resultados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en la búsqueda de similitud");
                return new List<ResultadoBusqueda>();
            }
        }

        public async Task<bool> EliminarVectoresAsync(Guid manualId)
        {
            try
            {
                var filter = new Filter
                {
                    Must = { new Condition { Field = new FieldCondition { Key = "manual_id", Match = new Match { Keyword = manualId.ToString() } } } }
                };

                await _client.DeleteAsync(_collectionName, filter);
                _logger.LogInformation("🗑️ Vectores eliminados para manual: {ManualId}", manualId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al eliminar vectores");
                return false;
            }
        }

        public async Task<bool> ExistenVectoresAsync(Guid manualId)
        {
            try
            {
                var filter = new Filter
                {
                    Must = { new Condition { Field = new FieldCondition { Key = "manual_id", Match = new Match { Keyword = manualId.ToString() } } } }
                };

                var result = await _client.ScrollAsync(_collectionName, filter, limit: 1);
                return result.Result.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar existencia de vectores");
                return false;
            }
        }
    }
}
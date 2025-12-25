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
        private readonly ILogger<QdrantService> _logger;

        public QdrantService(IConfiguration configuration, ILogger<QdrantService> logger)
        {
            _logger = logger;

            var host = configuration["Qdrant:Host"] ?? "localhost";
            var port = int.Parse(configuration["Qdrant:Port"] ?? "6334");
            _collectionName = configuration["Qdrant:CollectionName"] ?? "base_conocimiento";

            _client = new QdrantClient(host, port);

            _logger.LogInformation("Qdrant conectado: {Host}:{Port}, Colección: {Collection}",
                host, port, _collectionName);

            InicializarColeccionAsync().GetAwaiter().GetResult();
        }

        private async Task InicializarColeccionAsync()
        {
            try
            {
          
                bool existe = false;
                try
                {
                    var collectionInfo = await _client.GetCollectionInfoAsync(_collectionName);
                    existe = collectionInfo != null;
                }
                catch
                {
                
                    existe = false;
                }

                if (!existe)
                {
                    await _client.CreateCollectionAsync(
                        _collectionName,
                        new VectorParams
                        {
                            Size = 768,
                            Distance = Distance.Cosine
                        }
                    );

                    _logger.LogInformation("Colección Qdrant creada: {CollectionName}", _collectionName);
                }
                else
                {
                    _logger.LogInformation("Colección Qdrant ya existe: {CollectionName}", _collectionName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar colección Qdrant");
                throw;
            }
        }

        public async Task AlmacenarVectoresAsync(Guid manualId, List<VectorChunk> chunks)
        {
            try
            {
                if (!chunks.Any())
                {
                    _logger.LogWarning("No hay chunks para almacenar para manual {ManualId}", manualId);
                    return;
                }

                var points = new List<PointStruct>();

                foreach (var chunk in chunks)
                {
                    var pointId = Guid.NewGuid();

                    var payload = new Dictionary<string, Value>
                    {
                        { "manual_id", manualId.ToString() },
                        { "texto_original", chunk.TextoOriginal },
                        { "numero_pagina", chunk.NumeroPagina },
                        { "numero_chunk", chunk.NumeroChunk }
                    };

                    var point = new PointStruct
                    {
                        Id = pointId,
                        Vectors = chunk.Vector,
                        Payload = { payload }
                    };

                    points.Add(point);
                }

                await _client.UpsertAsync(_collectionName, points);

                _logger.LogInformation("Vectores almacenados en Qdrant: {ManualId}, {Count} chunks",
                    manualId, chunks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al almacenar vectores en Qdrant para manual {ManualId}", manualId);
                throw;
            }
        }

        public async Task<bool> EliminarVectoresAsync(Guid manualId)
        {
            try
            {
                var filter = new Filter
                {
                    Must =
                    {
                        new Condition
                        {
                            Field = new FieldCondition
                            {
                                Key = "manual_id",
                                Match = new Match { Keyword = manualId.ToString() }
                            }
                        }
                    }
                };

                await _client.DeleteAsync(_collectionName, filter);

                _logger.LogInformation("Vectores eliminados de Qdrant: {ManualId}", manualId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar vectores de Qdrant para manual {ManualId}", manualId);
                return false;
            }
        }

        public async Task<List<ResultadoBusqueda>> BuscarSimilaresAsync(
            float[] embedding,
            int topK = 5,
            string categoria = null)
        {
            try
            {
                Filter filter = null;

                if (!string.IsNullOrEmpty(categoria))
                {
                    filter = new Filter
                    {
                        Must =
                        {
                            new Condition
                            {
                                Field = new FieldCondition
                                {
                                    Key = "categoria",
                                    Match = new Match { Keyword = categoria }
                                }
                            }
                        }
                    };
                }

                var searchResult = await _client.SearchAsync(
                    _collectionName,
                    embedding,
                    filter: filter,
                    limit: (ulong)topK,
                    scoreThreshold: 0.3f
                );

                var resultados = searchResult.Select(r => new ResultadoBusqueda
                {
                    ManualId = Guid.Parse(r.Payload["manual_id"].StringValue),
                    Titulo = r.Payload.ContainsKey("titulo") ? r.Payload["titulo"].StringValue : "",
                    TextoOriginal = r.Payload["texto_original"].StringValue,
                    NumeroPagina = (int)r.Payload["numero_pagina"].IntegerValue,
                    Score = r.Score
                }).ToList();

                _logger.LogInformation("Búsqueda en Qdrant: {ResultCount} resultados encontrados",
                    resultados.Count);

                return resultados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar similares en Qdrant");
                return new List<ResultadoBusqueda>();
            }
        }

        public async Task<bool> ExistenVectoresAsync(Guid manualId)
        {
            try
            {
                var filter = new Filter
                {
                    Must =
                    {
                        new Condition
                        {
                            Field = new FieldCondition
                            {
                                Key = "manual_id",
                                Match = new Match { Keyword = manualId.ToString() }
                            }
                        }
                    }
                };

                var result = await _client.ScrollAsync(
                    _collectionName,
                    filter,
                    limit: 1
                );

                return result.Result.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar vectores en Qdrant para manual {ManualId}", manualId);
                return false;
            }
        }
    }
}
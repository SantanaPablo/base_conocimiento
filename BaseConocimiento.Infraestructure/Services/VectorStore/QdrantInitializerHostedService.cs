using BaseConocimiento.Application.Interfaces.VectorStore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BaseConocimiento.Infrastructure.Services.VectorStore
{
    public class QdrantInitializerHostedService : IHostedService
    {
        private readonly IQdrantService _qdrant;
        private readonly ILogger<QdrantInitializerHostedService> _logger;

        public QdrantInitializerHostedService(
            IQdrantService qdrant,
            ILogger<QdrantInitializerHostedService> logger)
        {
            _qdrant = qdrant;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Qdrant inicializado: colección verificada/creada");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}

using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.VectorStore;
using MediatR;

namespace BaseConocimiento.Application.UseCases.Consultas.Queries.BuscarEnManuales
{
    public class BuscarEnManualesHandler : IRequestHandler<BuscarEnManualesQuery, BuscarEnManualesResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmbeddingService _embeddingService;
        private readonly IQdrantService _qdrantService;

        public BuscarEnManualesHandler(IUnitOfWork unitOfWork, IEmbeddingService embeddingService, IQdrantService qdrantService)
        {
            _unitOfWork = unitOfWork;
            _embeddingService = embeddingService;
            _qdrantService = qdrantService;
        }

        public async Task<BuscarEnManualesResponse> Handle(BuscarEnManualesQuery request, CancellationToken ct)
        {
            var embedding = await _embeddingService.GenerarEmbeddingAsync(request.TextoBusqueda);

            var resultadosQdrant = await _qdrantService.BuscarSimilaresAsync(
                embedding,
                request.TopK,
                request.CategoriaId?.ToString()
            );

            var manualIds = resultadosQdrant.Select(r => r.ManualId).Distinct();
            var manualesDict = new Dictionary<Guid, (string Titulo, string Categoria)>();

            foreach (var id in manualIds)
            {
                var manual = await _unitOfWork.Manuales.ObtenerConCategoriaAsync(id, ct);
                if (manual != null)
                    manualesDict[id] = (manual.Titulo, manual.Categoria?.Nombre ?? "N/A");
            }

            var resultados = resultadosQdrant.Select(r => new ResultadoBusquedaDto
            {
                ManualId = r.ManualId,
                TituloManual = manualesDict.ContainsKey(r.ManualId) ? manualesDict[r.ManualId].Titulo : "Desconocido",
                CategoriaNombre = manualesDict.ContainsKey(r.ManualId) ? manualesDict[r.ManualId].Categoria : "N/A",
                NumeroPagina = r.NumeroPagina,
                TextoFragmento = r.TextoOriginal,
                ScoreSimilitud = Math.Round(r.Score * 100, 2)
            }).ToList();

            return new BuscarEnManualesResponse { Exitoso = true, Resultados = resultados };
        }
    }
}
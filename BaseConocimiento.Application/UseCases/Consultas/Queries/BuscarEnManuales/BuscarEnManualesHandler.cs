using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.Interfaces.VectorStore;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Consultas.Queries.BuscarEnManuales
{
    public class BuscarEnManualesHandler : IRequestHandler<BuscarEnManualesQuery, BuscarEnManualesResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmbeddingService _embeddingService;
        private readonly IQdrantService _qdrantService;

        public BuscarEnManualesHandler(
            IUnitOfWork unitOfWork,
            IEmbeddingService embeddingService,
            IQdrantService qdrantService)
        {
            _unitOfWork = unitOfWork;
            _embeddingService = embeddingService;
            _qdrantService = qdrantService;
        }

        public async Task<BuscarEnManualesResponse> Handle(
            BuscarEnManualesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var embedding = await _embeddingService.GenerarEmbeddingAsync(request.TextoBusqueda);

                var resultadosQdrant = await _qdrantService.BuscarSimilaresAsync(
                    embedding,
                    request.TopK,
                    request.Categoria
                );

                if (!resultadosQdrant.Any())
                {
                    return new BuscarEnManualesResponse
                    {
                        Exitoso = true,
                        Resultados = new List<ResultadoBusquedaDto>(),
                        Mensaje = "No se encontraron resultados"
                    };
                }

             
                var manualIds = resultadosQdrant.Select(r => r.ManualId).Distinct();
                var manualesDict = new Dictionary<Guid, (string Titulo, string Categoria)>();

                foreach (var id in manualIds)
                {
                    var manual = await _unitOfWork.Manuales.ObtenerPorIdAsync(id, cancellationToken);
                    if (manual != null)
                    {
                        manualesDict[id] = (manual.Titulo, manual.Categoria);
                    }
                }

                var resultados = resultadosQdrant.Select(r =>
                {
                    var (titulo, categoria) = manualesDict.ContainsKey(r.ManualId)
                        ? manualesDict[r.ManualId]
                        : ("Desconocido", "N/A");

                    return new ResultadoBusquedaDto
                    {
                        ManualId = r.ManualId,
                        TituloManual = titulo,
                        Categoria = categoria,
                        NumeroPagina = r.NumeroPagina,
                        TextoFragmento = r.TextoOriginal.Length > 300
                            ? r.TextoOriginal.Substring(0, 300) + "..."
                            : r.TextoOriginal,
                        ScoreSimilitud = Math.Round(r.Score * 100, 2)
                    };
                }).ToList();

                return new BuscarEnManualesResponse
                {
                    Exitoso = true,
                    Resultados = resultados,
                    Mensaje = $"Se encontraron {resultados.Count} resultados"
                };
            }
            catch (Exception ex)
            {
                return new BuscarEnManualesResponse
                {
                    Exitoso = false,
                    Resultados = new List<ResultadoBusquedaDto>(),
                    Mensaje = $"Error en la búsqueda: {ex.Message}"
                };
            }
        }
    }
}

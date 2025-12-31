using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.UseCases.Manuales.Queries.ListarManuales;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BaseConocimiento.Application.UseCases.Manuales.Queries.ListarManuales
{
    public class ListarManualesHandler : IRequestHandler<ListarManualesQuery, ListarManualesResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListarManualesHandler> _logger;

        public ListarManualesHandler(IUnitOfWork unitOfWork, ILogger<ListarManualesHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ListarManualesResponse> Handle(ListarManualesQuery request, CancellationToken ct)
        {
            try
            {
                var (manuales, total) = await _unitOfWork.Manuales.ListarPaginadoAsync(
                categoriaId: request.CategoriaId,
                terminoBusqueda: request.TerminoBusqueda,
                 pagina: request.Pagina,
                 tamañoPagina: request.TamañoPagina,
                ordenarPor: request.OrdenarPor.ToString(),
    ct
);

                var manualesDto = manuales.Select(m => new ManualListadoDto
                {
                    Id = m.Id,
                    Titulo = m.Titulo,
                    Categoria = m.Categoria?.Nombre ?? "Sin categoría",
                    SubCategoria = m.SubCategoria,
                    Version = m.Version,
                    FechaSubida = m.FechaSubida,
                    SubidoPor = m.Usuario?.NombreCompleto ?? "Usuario desconocido",
                    TamañoBytes = m.PesoArchivo,
                    TamañoFormateado = FormatearTamaño(m.PesoArchivo),
                    NumeroConsultas = m.NumeroConsultas,
                    UltimaConsulta = m.UltimaConsulta,
                    Estado = m.Estado.ToString()
                }).ToList();

                var totalPaginas = (int)Math.Ceiling(total / (double)request.TamañoPagina);

                return new ListarManualesResponse
                {
                    Exitoso = true,
                    Manuales = manualesDto,
                    TotalRegistros = total,
                    PaginaActual = request.Pagina,
                    TotalPaginas = totalPaginas
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar manuales");
                return new ListarManualesResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error: {ex.Message}"
                };
            }
        }

        private string FormatearTamaño(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
using BaseConocimiento.Application.Interfaces.Persistence;
using BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerManual;
using BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerManualPorId;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerManual
{
    public class ObtenerManualPorIdHandler : IRequestHandler<ObtenerManualPorIdQuery, ObtenerManualPorIdResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObtenerManualPorIdHandler> _logger;

        public ObtenerManualPorIdHandler(IUnitOfWork unitOfWork, ILogger<ObtenerManualPorIdHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ObtenerManualPorIdResponse> Handle(ObtenerManualPorIdQuery request, CancellationToken ct)
        {
            try
            {
                var manual = await _unitOfWork.Manuales.ObtenerConDetallesAsync(request.ManualId, ct);

                if (manual == null)
                {
                    return new ObtenerManualPorIdResponse
                    {
                        Exitoso = false,
                        Mensaje = "Manual no encontrado"
                    };
                }

                return new ObtenerManualPorIdResponse
                {
                    Exitoso = true,
                    Manual = new ManualDetalleDto
                    {
                        Id = manual.Id,
                        Titulo = manual.Titulo,
                        Descripcion = manual.Descripcion,
                        Categoria = manual.Categoria?.Nombre ?? "Sin categoría",
                        CategoriaId = manual.CategoriaId,
                        SubCategoria = manual.SubCategoria,
                        Version = manual.Version,
                        FechaSubida = manual.FechaSubida,
                        SubidoPor = manual.Usuario?.NombreCompleto ?? "Usuario desconocido",
                        EmailUsuario = manual.Usuario?.Email ?? "N/A",
                        TamañoBytes = manual.PesoArchivo,
                        TamañoFormateado = FormatearTamaño(manual.PesoArchivo),
                        NombreOriginal = manual.NombreOriginal,
                        NumeroConsultas = manual.NumeroConsultas,
                        UltimaConsulta = manual.UltimaConsulta,
                        Estado = manual.Estado.ToString(),
                        RutaDescarga = $"/api/manuales/{manual.Id}/descargar"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener manual: {ManualId}", request.ManualId);
                return new ObtenerManualPorIdResponse
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
using BaseConocimiento.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    namespace BaseConocimiento.Application.UseCases.Manuales.Queries.ObtenerManualPorId
    {
        public class ObtenerManualPorIdQuery : IRequest<ObtenerManualPorIdResponse>
        {
            public Guid ManualId { get; set; }
        }

        public class ObtenerManualPorIdResponse
        {
            public bool Exitoso { get; set; }
            public ManualDetalleDto? Manual { get; set; }
            public string Mensaje { get; set; }
        }

        public class ManualDetalleDto
        {
            public Guid Id { get; set; }
            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            public string Categoria { get; set; }
            public Guid CategoriaId { get; set; }
            public string? SubCategoria { get; set; }
            public string Version { get; set; }
            public DateTime FechaSubida { get; set; }
            public string SubidoPor { get; set; }
            public string EmailUsuario { get; set; }
            public long TamañoBytes { get; set; }
            public string TamañoFormateado { get; set; }
            public string NombreOriginal { get; set; }
            public int NumeroConsultas { get; set; }
            public DateTime? UltimaConsulta { get; set; }
            public string Estado { get; set; }
            public string RutaDescarga { get; set; }
        }
    }

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Consultas.Queries.ConsultarBaseConocimiento
{
    public class ConsultarBaseConocimientoQuery : IRequest<ConsultarBaseConocimientoResponse>
    {
        public string Pregunta { get; set; }
        public string Categoria { get; set; }
        public int TopK { get; set; } = 5;
    }

    public class ConsultarBaseConocimientoResponse
    {
        public string Respuesta { get; set; }
        public List<FuenteConsultada> Fuentes { get; set; }
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
    }

    public class FuenteConsultada
    {
        public Guid ManualId { get; set; }
        public string Titulo { get; set; }
        public int NumeroPagina { get; set; }
        public double Relevancia { get; set; }
        public string TextoFragmento { get; set; }
    }
}

using MediatR;

namespace BaseConocimiento.Application.UseCases.Consultas.Queries.ConsultarBaseConocimiento
{
    public class ConsultarBaseConocimientoQuery : IRequest<ConsultarBaseConocimientoResponse>
    {
        public string Pregunta { get; set; }
        public Guid? CategoriaId { get; set; }
        public int TopK { get; set; } = 5;
    }

    public class ConsultarBaseConocimientoResponse
    {
        public string Respuesta { get; set; }
        public List<FuenteConsultadaDto> Fuentes { get; set; }
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
    }

    public class FuenteConsultadaDto
    {
        public Guid ManualId { get; set; }
        public string Titulo { get; set; }
        public int NumeroPagina { get; set; }
        public double Relevancia { get; set; }
        public string TextoFragmento { get; set; }
    }
}
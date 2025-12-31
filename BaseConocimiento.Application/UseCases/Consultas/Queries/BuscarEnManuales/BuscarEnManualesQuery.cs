using MediatR;
using System;
using System.Collections.Generic;

namespace BaseConocimiento.Application.UseCases.Consultas.Queries.BuscarEnManuales
{
    public class BuscarEnManualesQuery : IRequest<BuscarEnManualesResponse>
    {
        public string TextoBusqueda { get; set; }

        public Guid? CategoriaId { get; set; }

        public int TopK { get; set; } = 10;
    }

    public class BuscarEnManualesResponse
    {
        public List<ResultadoBusquedaDto> Resultados { get; set; }
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
    }

    public class ResultadoBusquedaDto
    {
        public Guid ManualId { get; set; }
        public string TituloManual { get; set; }

        public string CategoriaNombre { get; set; }

        public int NumeroPagina { get; set; }
        public string TextoFragmento { get; set; }
        public double ScoreSimilitud { get; set; }
    }
}
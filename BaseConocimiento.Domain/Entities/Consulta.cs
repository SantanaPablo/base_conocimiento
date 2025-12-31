using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Domain.Entities
{
    public class Consulta
    {
        public Guid Id { get; private set; }
        public string Pregunta { get; private set; }
        public string Respuesta { get; private set; }
        public string ConversacionId { get; private set; }
        public Guid UsuarioId { get; private set; }
        public int ResultadosEncontrados { get; private set; }
        public long TiempoRespuestaMs { get; private set; }
        public DateTime FechaConsulta { get; private set; }
        public bool FueUtil { get; private set; }

        public Usuario Usuario { get; private set; }
        public ICollection<ConsultaManual> ManualesConsultados { get; private set; } = new List<ConsultaManual>();

        private Consulta() { }

        public static Consulta Crear(
            string pregunta,
            string respuesta,
            string conversacionId,
            Guid usuarioId,
            int resultadosEncontrados,
            long tiempoMs)
        {
            return new Consulta
            {
                Id = Guid.NewGuid(),
                Pregunta = pregunta,
                Respuesta = respuesta,
                ConversacionId = conversacionId,
                UsuarioId = usuarioId,
                ResultadosEncontrados = resultadosEncontrados,
                TiempoRespuestaMs = tiempoMs,
                FechaConsulta = DateTime.UtcNow,
                FueUtil = true
            };
        }

        public void AgregarManualConsultado(Guid manualId, double relevancia)
        {
            var relacion = new ConsultaManual
            {
                ConsultaId = Id,
                ManualId = manualId,
                RelevanciaPromedio = relevancia
            };
            ManualesConsultados.Add(relacion);
        }

        public void MarcarComoUtil() => FueUtil = true;
        public void MarcarComoNoUtil() => FueUtil = false;
    }
}

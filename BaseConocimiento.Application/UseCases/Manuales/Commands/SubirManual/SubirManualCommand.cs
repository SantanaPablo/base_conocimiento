using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Manuales.Commands.SubirManual
{
    public class SubirManualCommand : IRequest<SubirManualResponse>
    {
        public Stream ArchivoStream { get; set; }
        public string NombreOriginal { get; set; }
        public string Titulo { get; set; }
        public string Categoria { get; set; }
        public string SubCategoria { get; set; }
        public string Version { get; set; }
        public string Descripcion { get; set; }
        public string UsuarioId { get; set; }
    }

    public class SubirManualResponse
    {
        public Guid ManualId { get; set; }
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
        public int ChunksProcesados { get; set; }
    }
}

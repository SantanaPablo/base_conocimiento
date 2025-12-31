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
        public string Titulo { get; set; }
        public Guid CategoriaId { get; set; }
        public string Version { get; set; }
        public string? Descripcion { get; set; }
        public string NombreOriginal { get; set; }
        public Guid UsuarioId { get; set; }
        public Stream ArchivoStream { get; set; }

        public long PesoArchivo { get; set; }
    }

    public class SubirManualResponse
    {
        public bool Exitoso { get; set; }
        public Guid ManualId { get; set; }
        public int ChunksProcesados { get; set; }
        public string Mensaje { get; set; }
    }
}

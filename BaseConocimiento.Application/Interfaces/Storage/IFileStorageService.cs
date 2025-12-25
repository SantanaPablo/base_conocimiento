using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.Interfaces.Storage
{
    public interface IFileStorageService
    {
        Task<string> GuardarArchivoAsync(Guid manualId, Stream archivoStream, string nombreOriginal);
        Task<bool> EliminarArchivoAsync(Guid manualId);
        Task<Stream> ObtenerArchivoAsync(Guid manualId);
        Task<bool> ExisteArchivoAsync(Guid manualId);
    }
}

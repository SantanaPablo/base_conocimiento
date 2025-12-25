using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.Interfaces.AI
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerarEmbeddingAsync(string texto);
        Task<List<float[]>> GenerarEmbeddingsAsync(List<string> textos);
    }
}

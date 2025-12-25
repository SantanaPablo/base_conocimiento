using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.Interfaces.AI
{
    public interface IChatCompletionService
    {
        Task<string> GenerarRespuestaAsync(string prompt);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.Interfaces.Processing
{
    public interface IPdfProcessingService
    {
        Task<List<TextoExtraido>> ExtraerTextoAsync(Stream pdfStream);
    }

    public class TextoExtraido
    {
        public string Texto { get; set; }
        public int NumeroPagina { get; set; }
    }
}

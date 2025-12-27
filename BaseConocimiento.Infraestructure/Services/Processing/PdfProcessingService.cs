// Infrastructure/Services/Processing/PdfProcessingService.cs
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Logging;
using System.Text;
using BaseConocimiento.Application.Interfaces.Processing;

namespace BaseConocimiento.Infrastructure.Services.Processing
{
    public class PdfProcessingService : IPdfProcessingService
    {
        private readonly ILogger<PdfProcessingService> _logger;
        private const int MAX_CHUNK_SIZE = 500;
        private const int OVERLAP_SIZE = 100;
        private const int MIN_CHUNK_SIZE = 100;

        public PdfProcessingService(ILogger<PdfProcessingService> logger)
        {
            _logger = logger;
        }

        public async Task<List<TextoExtraido>> ExtraerTextoAsync(Stream pdfStream)
        {
            var textosExtraidos = new List<TextoExtraido>();

            try
            {
                await Task.Run(() =>
                {
                    using var pdfReader = new PdfReader(pdfStream);
                    using var pdfDocument = new PdfDocument(pdfReader);

                    int totalPaginas = pdfDocument.GetNumberOfPages();
                    _logger.LogInformation("Procesando PDF: {TotalPaginas} páginas", totalPaginas);

                    for (int pagina = 1; pagina <= totalPaginas; pagina++)
                    {
                        try
                        {
                            var page = pdfDocument.GetPage(pagina);
                            var strategy = new SimpleTextExtractionStrategy();
                            var texto = PdfTextExtractor.GetTextFromPage(page, strategy);

                            texto = LimpiarTexto(texto);

                            if (string.IsNullOrWhiteSpace(texto) || texto.Length < MIN_CHUNK_SIZE)
                            {
                                _logger.LogWarning("Página {Pagina} tiene poco contenido, saltando", pagina);
                                continue;
                            }
                            var chunks = DividirEnChunksConOverlap(texto, MAX_CHUNK_SIZE, OVERLAP_SIZE);

                            foreach (var chunk in chunks)
                            {
                                textosExtraidos.Add(new TextoExtraido
                                {
                                    Texto = chunk,
                                    NumeroPagina = pagina
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al procesar página {Pagina}", pagina);
                        }
                    }
                });

                _logger.LogInformation("Extracción completa: {TotalChunks} chunks generados",
                    textosExtraidos.Count);

                return textosExtraidos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extraer texto del PDF");
                throw;
            }
        }

        private string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"\s+", " ");
            texto = texto.Replace("\r\n", " ")
                        .Replace("\n", " ")
                        .Replace("\r", " ")
                        .Replace("\t", " ");
            texto = texto.Trim();

            return texto;
        }

        // ✅ NUEVO: Dividir con overlap para mantener contexto
        private List<string> DividirEnChunksConOverlap(string texto, int tamañoChunk, int overlap)
        {
            var chunks = new List<string>();
            var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int inicio = 0;
            while (inicio < palabras.Length)
            {
                var chunkPalabras = new List<string>();
                int longitudActual = 0;

                // Tomar palabras hasta alcanzar el tamaño
                for (int i = inicio; i < palabras.Length; i++)
                {
                    var palabra = palabras[i];
                    if (longitudActual + palabra.Length + 1 > tamañoChunk && chunkPalabras.Any())
                        break;

                    chunkPalabras.Add(palabra);
                    longitudActual += palabra.Length + 1;
                }

                if (chunkPalabras.Any())
                {
                    var chunk = string.Join(" ", chunkPalabras);
                    if (chunk.Length >= MIN_CHUNK_SIZE)
                        chunks.Add(chunk);
                }

                // Calcular siguiente inicio con overlap
                int palabrasUsadas = chunkPalabras.Count;
                int palabrasOverlap = (int)(palabrasUsadas * ((double)overlap / tamañoChunk));
                inicio += Math.Max(1, palabrasUsadas - palabrasOverlap);
            }

            return chunks;
        }
    }

    public class TextProcessingService : ITextProcessingService
    {
        private readonly ILogger<TextProcessingService> _logger;
        private const int MAX_CHUNK_SIZE = 500;
        private const int OVERLAP_SIZE = 100;
        private const int MIN_CHUNK_SIZE = 100;

        public TextProcessingService(ILogger<TextProcessingService> logger)
        {
            _logger = logger;
        }

        public Task<List<TextoExtraido>> ExtraerTextoAsync(Stream textStream)
        {
            var textosExtraidos = new List<TextoExtraido>();

            try
            {
                using var reader = new StreamReader(textStream, Encoding.UTF8);
                var textoCompleto = reader.ReadToEnd();

                if (string.IsNullOrWhiteSpace(textoCompleto))
                {
                    _logger.LogWarning("El archivo de texto está vacío");
                    return Task.FromResult(textosExtraidos);
                }

                // Limpiar
                textoCompleto = LimpiarTexto(textoCompleto);

                // Dividir con overlap
                var chunks = DividirEnChunksConOverlap(textoCompleto, MAX_CHUNK_SIZE, OVERLAP_SIZE);

                int numeroChunk = 1;
                foreach (var chunk in chunks)
                {
                    textosExtraidos.Add(new TextoExtraido
                    {
                        Texto = chunk,
                        NumeroPagina = 1 // Para TXT todo es "página 1"
                    });
                    numeroChunk++;
                }

                _logger.LogInformation("Texto procesado: {TotalChunks} chunks generados", textosExtraidos.Count);

                return Task.FromResult(textosExtraidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar archivo de texto");
                throw;
            }
        }

        private string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"\s+", " ");
            return texto.Trim();
        }

        private List<string> DividirEnChunksConOverlap(string texto, int tamañoChunk, int overlap)
        {
            var chunks = new List<string>();
            var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int inicio = 0;
            while (inicio < palabras.Length)
            {
                var chunkPalabras = new List<string>();
                int longitudActual = 0;

                for (int i = inicio; i < palabras.Length; i++)
                {
                    var palabra = palabras[i];
                    if (longitudActual + palabra.Length + 1 > tamañoChunk && chunkPalabras.Any())
                        break;

                    chunkPalabras.Add(palabra);
                    longitudActual += palabra.Length + 1;
                }

                if (chunkPalabras.Any())
                {
                    var chunk = string.Join(" ", chunkPalabras);
                    if (chunk.Length >= MIN_CHUNK_SIZE)
                        chunks.Add(chunk);
                }

                int palabrasUsadas = chunkPalabras.Count;
                int palabrasOverlap = (int)(palabrasUsadas * ((double)overlap / tamañoChunk));
                inicio += Math.Max(1, palabrasUsadas - palabrasOverlap);
            }

            return chunks;
        }
    }
}
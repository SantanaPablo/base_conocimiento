using BaseConocimiento.Application.Interfaces.Processing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace BaseConocimiento.Infrastructure.Services.Processing
{
    public class PdfProcessingService : IPdfProcessingService
    {
        private readonly ILogger<PdfProcessingService> _logger;
        private const int MAX_CHUNK_SIZE = 500;
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

                            var chunks = DividirEnChunks(texto, MAX_CHUNK_SIZE);

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

        private List<string> DividirEnChunks(string texto, int tamañoMaximo)
        {
            var chunks = new List<string>();

            if (string.IsNullOrWhiteSpace(texto))
                return chunks;

            var oraciones = DividirPorOraciones(texto);

            var chunkActual = new StringBuilder();

            foreach (var oracion in oraciones)
            {
                if (chunkActual.Length + oracion.Length + 1 > tamañoMaximo)
                {
                    if (chunkActual.Length > 0)
                    {
                        chunks.Add(chunkActual.ToString().Trim());
                        chunkActual.Clear();
                    }

                    if (oracion.Length > tamañoMaximo)
                    {
                        var subChunks = DividirPorPalabras(oracion, tamañoMaximo);
                        chunks.AddRange(subChunks);
                    }
                    else
                    {
                        chunkActual.Append(oracion).Append(' ');
                    }
                }
                else
                {
                    chunkActual.Append(oracion).Append(' ');
                }
            }

            if (chunkActual.Length > 0)
            {
                chunks.Add(chunkActual.ToString().Trim());
            }

            return chunks.Where(c => c.Length >= MIN_CHUNK_SIZE).ToList();
        }

        private List<string> DividirPorOraciones(string texto)
        {
            var delimitadores = new[] { ". ", "? ", "! ", ".\n", "?\n", "!\n" };
            var oraciones = new List<string>();
            var oracionActual = new StringBuilder();

            for (int i = 0; i < texto.Length; i++)
            {
                oracionActual.Append(texto[i]);

                bool esDelimitador = false;
                foreach (var delim in delimitadores)
                {
                    if (i >= delim.Length - 1)
                    {
                        var substring = texto.Substring(i - delim.Length + 1, delim.Length);
                        if (substring == delim)
                        {
                            esDelimitador = true;
                            break;
                        }
                    }
                }

                if (esDelimitador)
                {
                    var oracion = oracionActual.ToString().Trim();
                    if (oracion.Length > 0)
                    {
                        oraciones.Add(oracion);
                    }
                    oracionActual.Clear();
                }
            }

            if (oracionActual.Length > 0)
            {
                oraciones.Add(oracionActual.ToString().Trim());
            }

            return oraciones;
        }

        private List<string> DividirPorPalabras(string texto, int tamañoMaximo)
        {
            var chunks = new List<string>();
            var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var chunkActual = new StringBuilder();

            foreach (var palabra in palabras)
            {
                if (chunkActual.Length + palabra.Length + 1 > tamañoMaximo)
                {
                    if (chunkActual.Length > 0)
                    {
                        chunks.Add(chunkActual.ToString().Trim());
                        chunkActual.Clear();
                    }
                }

                chunkActual.Append(palabra).Append(' ');
            }

            if (chunkActual.Length > 0)
            {
                chunks.Add(chunkActual.ToString().Trim());
            }

            return chunks;
        }
    }
}

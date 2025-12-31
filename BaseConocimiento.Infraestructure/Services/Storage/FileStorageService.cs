using BaseConocimiento.Application.Interfaces.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Infrastructure.Services.Storage
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _rutaBase;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
        {
            _rutaBase = configuration["FileStorage:LocalPath"] ?? "C:/KnowledgeBase";
            _logger = logger;

            if (!Directory.Exists(_rutaBase))
            {
                Directory.CreateDirectory(_rutaBase);
                _logger.LogInformation("Directorio base creado: {RutaBase}", _rutaBase);
            }
        }

        public async Task<string> GuardarArchivoAsync(Guid manualId, Stream archivoStream, string nombreOriginal)
        {
            try
            {
                var extension = Path.GetExtension(nombreOriginal);
                var nombreArchivo = $"{manualId}{extension}";
                var rutaCompleta = Path.Combine(_rutaBase, nombreArchivo);

                using (var fileStream = new FileStream(rutaCompleta, FileMode.Create, FileAccess.Write))
                {
                    await archivoStream.CopyToAsync(fileStream);
                }

                _logger.LogInformation("Archivo guardado: {RutaCompleta}", rutaCompleta);
                return rutaCompleta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar archivo {ManualId}", manualId);
                throw;
            }
        }

        public Task<bool> EliminarArchivoAsync(Guid manualId)
        {
            try
            {
                var archivos = Directory.GetFiles(_rutaBase, $"{manualId}.*");

                if (archivos.Length == 0)
                {
                    _logger.LogWarning("No se encontró archivo para eliminar: {ManualId}", manualId);
                    return Task.FromResult(false);
                }

                foreach (var archivo in archivos)
                {
                    File.Delete(archivo);
                    _logger.LogInformation("Archivo eliminado: {Archivo}", archivo);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar archivo {ManualId}", manualId);
                return Task.FromResult(false);
            }
        }

        public Task<Stream> ObtenerArchivoAsync(Guid manualId)
        {
            try
            {
                var archivos = Directory.GetFiles(_rutaBase, $"{manualId}.*");

                if (archivos.Length == 0)
                {
                    _logger.LogWarning("Archivo no encontrado: {ManualId}", manualId);
                    throw new FileNotFoundException($"Archivo no encontrado para el manual {manualId}");
                }

                var stream = new FileStream(archivos[0], FileMode.Open, FileAccess.Read);
                return Task.FromResult<Stream>(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener archivo {ManualId}", manualId);
                throw;
            }
        }

        public Task<bool> ExisteArchivoAsync(Guid manualId)
        {
            var archivos = Directory.GetFiles(_rutaBase, $"{manualId}.*");
            return Task.FromResult(archivos.Length > 0);
        }
    }
}

using BaseConocimiento.Application.Interfaces.AI;
using BaseConocimiento.Application.Interfaces.Conversation;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using StackExchange.Redis;
using System.Text.Json;
using BaseConocimiento.Domain.Entities;

namespace BaseConocimiento.Infrastructure.Services.Conversation
{
    public class RedisConversationService : IConversationService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger<RedisConversationService> _logger;
        private readonly TimeSpan _expiracion = TimeSpan.FromDays(7); // Conversaciones expiran en 7 días

        public RedisConversationService(
            IConnectionMultiplexer redis,
            ILogger<RedisConversationService> logger)
        {
            _redis = redis;
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<string> CrearConversacionAsync(string usuarioId)
        {
            var conversacionId = Guid.NewGuid().ToString("N");

            var metadata = new ConversacionMetadata
            {
                ConversacionId = conversacionId,
                UsuarioId = usuarioId,
                FechaCreacion = DateTime.UtcNow,
                UltimaActividad = DateTime.UtcNow,
                TotalMensajes = 0
            };

            var key = GetMetadataKey(conversacionId);
            await _db.StringSetAsync(key, JsonSerializer.Serialize(metadata), _expiracion);

            _logger.LogInformation("✅ Conversación creada: {ConversacionId} para usuario {UsuarioId}",
                conversacionId, usuarioId);

            return conversacionId;
        }

        public async Task AgregarMensajeAsync(string conversacionId, string rol, string contenido)
        {
            var mensaje = new MensajeConversacion
            {
                Rol = rol,
                Contenido = contenido,
                Timestamp = DateTime.UtcNow
            };

            var mensajesKey = GetMensajesKey(conversacionId);
            var mensajeJson = JsonSerializer.Serialize(mensaje);

            // Agregar mensaje a la lista
            await _db.ListRightPushAsync(mensajesKey, mensajeJson);

            // Actualizar expiracion
            await _db.KeyExpireAsync(mensajesKey, _expiracion);

            // Actualizar metadata
            await ActualizarMetadataAsync(conversacionId);

            _logger.LogDebug("💬 Mensaje agregado [{Rol}]: {Contenido}",
                rol, contenido.Substring(0, Math.Min(50, contenido.Length)));
        }

        public async Task<List<MensajeConversacion>> ObtenerHistorialAsync(string conversacionId)
        {
            var mensajesKey = GetMensajesKey(conversacionId);
            var mensajesJson = await _db.ListRangeAsync(mensajesKey);

            var mensajes = new List<MensajeConversacion>();
            foreach (var mensajeJson in mensajesJson)
            {
                var mensaje = JsonSerializer.Deserialize<MensajeConversacion>(mensajeJson);
                if (mensaje != null)
                    mensajes.Add(mensaje);
            }

            return mensajes;
        }

        public async Task<List<MensajeConversacion>> ObtenerUltimosMensajesAsync(
            string conversacionId,
            int cantidad = 10)
        {
            var mensajesKey = GetMensajesKey(conversacionId);

            // Obtener últimos N mensajes
            var mensajesJson = await _db.ListRangeAsync(mensajesKey, -cantidad, -1);

            var mensajes = new List<MensajeConversacion>();
            foreach (var mensajeJson in mensajesJson)
            {
                var mensaje = JsonSerializer.Deserialize<MensajeConversacion>(mensajeJson);
                if (mensaje != null)
                    mensajes.Add(mensaje);
            }

            return mensajes;
        }

        public async Task LimpiarConversacionAsync(string conversacionId)
        {
            var mensajesKey = GetMensajesKey(conversacionId);
            var metadataKey = GetMetadataKey(conversacionId);

            await _db.KeyDeleteAsync(mensajesKey);
            await _db.KeyDeleteAsync(metadataKey);

            _logger.LogInformation("🗑️ Conversación limpiada: {ConversacionId}", conversacionId);
        }

        public async Task<bool> ExisteConversacionAsync(string conversacionId)
        {
            var metadataKey = GetMetadataKey(conversacionId);
            return await _db.KeyExistsAsync(metadataKey);
        }

        public async Task<ConversacionMetadata> ObtenerMetadataAsync(string conversacionId)
        {
            var metadataKey = GetMetadataKey(conversacionId);
            var metadataJson = await _db.StringGetAsync(metadataKey);

            if (metadataJson.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<ConversacionMetadata>(metadataJson);
        }

        private async Task ActualizarMetadataAsync(string conversacionId)
        {
            var metadata = await ObtenerMetadataAsync(conversacionId);
            if (metadata == null)
                return;

            metadata.UltimaActividad = DateTime.UtcNow;
            metadata.TotalMensajes++;

            var metadataKey = GetMetadataKey(conversacionId);
            await _db.StringSetAsync(
                metadataKey,
                JsonSerializer.Serialize(metadata),
                _expiracion
            );
        }

        private string GetMensajesKey(string conversacionId) => $"conv:{conversacionId}:msgs";
        private string GetMetadataKey(string conversacionId) => $"conv:{conversacionId}:meta";
    }
}

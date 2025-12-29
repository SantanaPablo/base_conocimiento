namespace BaseConocimiento.API.DTOs.Conversation
{
    public class CrearConversacionRequest
    {
        public string UsuarioId { get; set; }
    }

    public class PreguntarConversacionRequest
    {
        public string Pregunta { get; set; }
        public string ConversacionId { get; set; }
        public string UsuarioId { get; set; }
        public string? Categoria { get; set; }
        public int TopK { get; set; } = 5;
    }
}

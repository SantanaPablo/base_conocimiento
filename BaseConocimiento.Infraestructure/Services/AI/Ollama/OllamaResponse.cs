namespace BaseConocimiento.Infrastructure.Services.AI.Ollama
{
    public class OllamaResponse
    {
        public string? response { get; set; }
        public double[]? embedding { get; set; }
    }
    public class OllamaChatResponse
    {
        public OllamaMessage message { get; set; }
    }

    public class OllamaMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}
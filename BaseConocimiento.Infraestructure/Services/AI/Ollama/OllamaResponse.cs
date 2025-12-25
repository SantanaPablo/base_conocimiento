namespace BaseConocimiento.Infrastructure.Services.AI.Ollama
{
    public class OllamaResponse
    {
        public string? response { get; set; }
        public double[]? embedding { get; set; }
    }
}
namespace BaseConocimiento.Application.Interfaces.VectorStore
{
    public interface IQdrantService
    {
        Task AlmacenarVectoresAsync(Guid manualId, List<VectorChunk> chunks);
        Task<bool> EliminarVectoresAsync(Guid manualId);
        Task<List<ResultadoBusqueda>> BuscarSimilaresAsync(float[] embedding, int topK = 5, string categoriaId = null);
        Task<bool> ExistenVectoresAsync(Guid manualId);
    }

    public class VectorChunk
    {
        public float[] Vector { get; set; }
        public string TextoOriginal { get; set; }
        public int NumeroPagina { get; set; }
        public int NumeroChunk { get; set; }
        public string Categoria { get; set; }
        public string Titulo { get; set; }
    }

    public class ResultadoBusqueda
    {
        public Guid ManualId { get; set; }
        public string Titulo { get; set; }
        public string TextoOriginal { get; set; }
        public int NumeroPagina { get; set; }
        public double Score { get; set; }
    }
}

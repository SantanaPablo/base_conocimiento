namespace BaseConocimiento.Domain.ValueObjects
{
    /// <summary>
    /// Representa los metadatos que se almacenarán en Qdrant junto con cada vector
    /// </summary>
    public class MetadataQdrant
    {
        public Guid ManualId { get; set; }
        public string Titulo { get; set; }
        public string Categoria { get; set; }
        public string SubCategoria { get; set; }
        public int NumeroPagina { get; set; }
        public int NumeroChunk { get; set; }
        public string TextoOriginal { get; set; }

        public Dictionary<string, object> ToQdrantPayload()
        {
            return new Dictionary<string, object>
            {
                { "manual_id", ManualId.ToString() },
                { "titulo", Titulo },
                { "categoria", Categoria },
                { "sub_categoria", SubCategoria },
                { "numero_pagina", NumeroPagina },
                { "numero_chunk", NumeroChunk },
                { "texto_original", TextoOriginal }
            };
        }
    }
}
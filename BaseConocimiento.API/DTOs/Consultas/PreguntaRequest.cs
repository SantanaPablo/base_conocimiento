namespace BaseConocimiento.API.DTOs.Consultas
{
    public class PreguntaRequest
    {
        public string Pregunta { get; set; }
        public string Categoria { get; set; }
        public int TopK { get; set; } = 5;
    }

    public class BuscarRequest
    {
        public string TextoBusqueda { get; set; }
        public string Categoria { get; set; }
        public int TopK { get; set; } = 10;
    }
}

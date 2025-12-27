namespace BaseConocimiento.API.DTOs.Manuales
{
    public class SubirManualRequest
    {
        public IFormFile Archivo { get; set; }
        public string Titulo { get; set; }
        public string Categoria { get; set; }
        public string SubCategoria { get; set; }
        public string Version { get; set; }
        public string Descripcion { get; set; }
    }
}

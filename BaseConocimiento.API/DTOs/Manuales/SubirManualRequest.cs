namespace BaseConocimiento.API.DTOs.Manuales
{
    public class SubirManualRequest
    {
        public string Titulo { get; set; }
        public Guid CategoriaId { get; set; }
        public string? Version { get; set; }
        public string? Descripcion { get; set; }
        public Guid UsuarioId { get; set; }
        public IFormFile Archivo { get; set; }
    }
}

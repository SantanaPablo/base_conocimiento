namespace BaseConocimiento.API.DTOs.Auth
{
    public class LoginResponse
    {
        public bool Exitoso { get; set; }
        public string? Token { get; set; }
        public UsuarioInfoDto? Usuario { get; set; }
        public string Mensaje { get; set; }
    }

    public class UsuarioInfoDto
    {
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
    }
}

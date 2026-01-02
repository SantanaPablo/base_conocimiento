namespace BaseConocimiento.API.DTOs.Auth
{
    public class RegisterRequest
    {
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Departamento { get; set; }
    }
}

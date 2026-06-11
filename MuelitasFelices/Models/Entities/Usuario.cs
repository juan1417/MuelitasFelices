using Microsoft.AspNetCore.Identity;

namespace MuelitasFelices.Models.Entities
{
    public class Usuario : IdentityUser
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}

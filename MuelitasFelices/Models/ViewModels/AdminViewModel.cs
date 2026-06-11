using MuelitasFelices.Models.Entities;

namespace MuelitasFelices.Models.ViewModels
{
    public class UsuarioAdminViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
    }

    public class MedicoAdminViewModel
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NumeroLicencia { get; set; } = string.Empty;
        public List<string> Especialidades { get; set; } = new();
    }

    public class CitaAdminViewModel
    {
        public int Id { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public string MedicoNombre { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string TipoCita { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }

    public class DashboardAdminViewModel
    {
        public int TotalUsuarios { get; set; }
        public int TotalMedicos { get; set; }
        public int TotalPacientes { get; set; }
        public int TotalCitasPendientes { get; set; }
        public int TotalCitasHoy { get; set; }
        public List<CitaAdminViewModel> CitasRecientes { get; set; } = new();
    }

    public class DashboardPacienteViewModel
    {
        public int ProximasCitas { get; set; }
        public int HistorialCount { get; set; }
        public List<CitaViewModel> ProximasCitasDetalle { get; set; } = new();
    }

    public class DashboardMedicoViewModel
    {
        public int CitasHoy { get; set; }
        public int TotalPacientes { get; set; }
        public int CitasPendientes { get; set; }
        public List<CitaViewModel> CitasDelDia { get; set; } = new();
    }

    public class EditarUsuarioViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public List<string> RolesDisponibles { get; set; } = new();
    }
}

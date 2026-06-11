using System.ComponentModel.DataAnnotations;

namespace MuelitasFelices.Models.ViewModels
{
    public class HistorialViewModel
    {
        public int PacienteId { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public int CitaId { get; set; }
        public DateTime FechaCita { get; set; }
        public string MotivoCita { get; set; } = string.Empty;

        public int? HistorialId { get; set; }

        [Display(Name = "Diagnóstico")]
        public string Diagnostico { get; set; } = string.Empty;

        [Display(Name = "Tratamiento")]
        public string Tratamiento { get; set; } = string.Empty;

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = string.Empty;
    }

    public class PacienteHistorialViewModel
    {
        public int PacienteId { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public List<HistorialMedicoResumen> EntradasHistorial { get; set; } = new();
    }

    public class HistorialMedicoResumen
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string MedicoNombre { get; set; } = string.Empty;
        public string Diagnostico { get; set; } = string.Empty;
        public string Tratamiento { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
    }
}

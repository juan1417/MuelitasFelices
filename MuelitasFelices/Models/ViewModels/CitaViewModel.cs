using MuelitasFelices.Models.Entities;

namespace MuelitasFelices.Models.ViewModels
{
    public class CitaViewModel
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public string PacienteNombre { get; set; } = string.Empty;
        public string MedicoNombre { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string TipoCita { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public string? Notas { get; set; }
    }

    public class MisCitasViewModel
    {
        public List<CitaViewModel> ProximasCitas { get; set; } = new();
        public List<CitaViewModel> HistorialCitas { get; set; } = new();
    }
}

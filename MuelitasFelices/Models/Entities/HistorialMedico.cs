namespace MuelitasFelices.Models.Entities
{
    public class HistorialMedico
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public int MedicoId { get; set; }
        public int CitaId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string Diagnostico { get; set; } = string.Empty;
        public string Tratamiento { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        public Paciente Paciente { get; set; } = null!;
        public Medico Medico { get; set; } = null!;
        public Cita Cita { get; set; } = null!;
    }
}

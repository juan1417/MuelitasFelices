namespace MuelitasFelices.Models.Entities
{
    public enum TipoCita
    {
        General,
        PorHistorial,
        Remitido
    }

    public enum EstadoCita
    {
        Pendiente,
        Confirmada,
        Cancelada,
        Completada
    }

    public class Cita
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public int MedicoId { get; set; }
        public DateTime FechaHora { get; set; }
        public TipoCita TipoCita { get; set; }
        public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;
        public string Motivo { get; set; } = string.Empty;
        public string? Notas { get; set; }
        public int? CitaAnteriorId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public Paciente Paciente { get; set; } = null!;
        public Medico Medico { get; set; } = null!;
        public Cita? CitaAnterior { get; set; }
        public ICollection<Cita> CitasDerivadas { get; set; } = new List<Cita>();
        public HistorialMedico? HistorialMedico { get; set; }
    }
}

namespace MuelitasFelices.Models.Entities
{
    public class Paciente
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Direccion { get; set; } = string.Empty;

        public Usuario Usuario { get; set; } = null!;
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
        public ICollection<HistorialMedico> HistorialMedicos { get; set; } = new List<HistorialMedico>();
    }
}

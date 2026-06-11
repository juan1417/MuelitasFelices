namespace MuelitasFelices.Models.Entities
{
    public class Medico
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public string NumeroLicencia { get; set; } = string.Empty;

        public Usuario Usuario { get; set; } = null!;
        public ICollection<MedicoEspecialidad> MedicoEspecialidades { get; set; } = new List<MedicoEspecialidad>();
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
        public ICollection<HistorialMedico> HistorialMedicos { get; set; } = new List<HistorialMedico>();
    }
}

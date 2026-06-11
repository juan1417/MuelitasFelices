using System.ComponentModel.DataAnnotations;
using MuelitasFelices.Models.Entities;

namespace MuelitasFelices.Models.ViewModels
{
    public class AgendarCitaViewModel
    {
        [Required(ErrorMessage = "Seleccione el tipo de cita")]
        [Display(Name = "Tipo de Cita")]
        public TipoCita TipoCita { get; set; }

        public List<Especialidad> Especialidades { get; set; } = new();
        public int? EspecialidadId { get; set; }

        public List<MedicoDisponibleViewModel> MedicosDisponibles { get; set; } = new();
        public int? MedicoId { get; set; }

        [Required(ErrorMessage = "Seleccione una fecha")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Seleccione una hora")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora")]
        public TimeSpan Hora { get; set; }

        [Required(ErrorMessage = "El motivo es requerido")]
        [Display(Name = "Motivo de la consulta")]
        public string Motivo { get; set; } = string.Empty;

        public List<CitaResumenViewModel> CitasAnteriores { get; set; } = new();
        public int? CitaAnteriorId { get; set; }

        public List<TimeSpan> HorariosDisponibles { get; set; } = new();
    }

    public class MedicoDisponibleViewModel
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string NumeroLicencia { get; set; } = string.Empty;
        public List<string> Especialidades { get; set; } = new();
    }

    public class CitaResumenViewModel
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string MedicoNombre { get; set; } = string.Empty;
        public string TipoCita { get; set; } = string.Empty;
    }
}

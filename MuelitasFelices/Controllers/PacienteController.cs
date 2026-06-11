using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuelitasFelices.Data;
using MuelitasFelices.Models.Entities;
using MuelitasFelices.Models.ViewModels;

namespace MuelitasFelices.Controllers
{
    [Authorize(Roles = "Paciente")]
    public class PacienteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Usuario> _userManager;

        public PacienteController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.UsuarioId == user.Id);
            if (paciente == null) return NotFound();

            var proximasCitas = await _context.Citas
                .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                .Where(c => c.PacienteId == paciente.Id && c.FechaHora >= DateTime.UtcNow && c.Estado != EstadoCita.Cancelada)
                .OrderBy(c => c.FechaHora)
                .Take(5)
                .ToListAsync();

            var model = new DashboardPacienteViewModel
            {
                ProximasCitas = proximasCitas.Count,
                HistorialCount = await _context.Citas.CountAsync(c => c.PacienteId == paciente.Id && c.Estado == EstadoCita.Completada),
                ProximasCitasDetalle = proximasCitas.Select(c => new CitaViewModel
                {
                    Id = c.Id,
                    MedicoNombre = c.Medico.Usuario.NombreCompleto,
                    FechaHora = c.FechaHora,
                    TipoCita = c.TipoCita.ToString(),
                    Estado = c.Estado.ToString(),
                    Motivo = c.Motivo
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Agendar()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.UsuarioId == user.Id);
            if (paciente == null) return NotFound();

            var model = new AgendarCitaViewModel
            {
                Especialidades = await _context.Especialidades.ToListAsync(),
                CitasAnteriores = await _context.Citas
                    .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                    .Where(c => c.PacienteId == paciente.Id && c.Estado == EstadoCita.Completada)
                    .Select(c => new CitaResumenViewModel
                    {
                        Id = c.Id,
                        FechaHora = c.FechaHora,
                        MedicoNombre = c.Medico.Usuario.NombreCompleto,
                        TipoCita = c.TipoCita.ToString()
                    }).ToListAsync(),
                Fecha = DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(AgendarCitaViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.UsuarioId == user.Id);
            if (paciente == null) return NotFound();

            if (model.TipoCita == TipoCita.PorHistorial && model.CitaAnteriorId == null)
            {
                ModelState.AddModelError("CitaAnteriorId", "Seleccione una cita anterior");
            }

            if (model.TipoCita != TipoCita.PorHistorial && model.MedicoId == null)
            {
                ModelState.AddModelError("MedicoId", "Seleccione un médico");
            }

            if (!ModelState.IsValid)
            {
                model.Especialidades = await _context.Especialidades.ToListAsync();
                return View(model);
            }

            int medicoId;
            if (model.TipoCita == TipoCita.PorHistorial && model.CitaAnteriorId.HasValue)
            {
                var citaAnterior = await _context.Citas.FindAsync(model.CitaAnteriorId.Value);
                if (citaAnterior == null) return NotFound();
                medicoId = citaAnterior.MedicoId;
            }
            else
            {
                medicoId = model.MedicoId!.Value;
            }

            var cita = new Cita
            {
                PacienteId = paciente.Id,
                MedicoId = medicoId,
                FechaHora = model.Fecha.Date + model.Hora,
                TipoCita = model.TipoCita,
                Motivo = model.Motivo,
                Estado = EstadoCita.Pendiente,
                CitaAnteriorId = model.CitaAnteriorId
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cita agendada correctamente. Espere la confirmación.";
            return RedirectToAction(nameof(MisCitas));
        }

        public async Task<IActionResult> MisCitas()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.UsuarioId == user.Id);
            if (paciente == null) return NotFound();

            var citas = await _context.Citas
                .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                .Where(c => c.PacienteId == paciente.Id)
                .OrderByDescending(c => c.FechaHora)
                .ToListAsync();

            var model = new MisCitasViewModel
            {
                ProximasCitas = citas
                    .Where(c => c.FechaHora >= DateTime.UtcNow && c.Estado != EstadoCita.Cancelada)
                    .Select(MapToViewModel).ToList(),
                HistorialCitas = citas
                    .Where(c => c.FechaHora < DateTime.UtcNow || c.Estado == EstadoCita.Cancelada)
                    .Select(MapToViewModel).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            cita.Estado = EstadoCita.Cancelada;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cita cancelada correctamente.";
            return RedirectToAction(nameof(MisCitas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reprogramar(int id, DateTime nuevaFecha, TimeSpan nuevaHora)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            cita.FechaHora = nuevaFecha.Date + nuevaHora;
            cita.Estado = EstadoCita.Pendiente;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cita reprogramada correctamente.";
            return RedirectToAction(nameof(MisCitas));
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerMedicos(int especialidadId)
        {
            var medicos = await _context.MedicoEspecialidades
                .Include(me => me.Medico).ThenInclude(m => m.Usuario)
                .Include(me => me.Especialidad)
                .Where(me => me.EspecialidadId == especialidadId)
                .Select(me => new MedicoDisponibleViewModel
                {
                    Id = me.Medico.Id,
                    NombreCompleto = me.Medico.Usuario.NombreCompleto,
                    NumeroLicencia = me.Medico.NumeroLicencia,
                    Especialidades = me.Medico.MedicoEspecialidades.Select(mes => mes.Especialidad.Nombre).ToList()
                })
                .Distinct()
                .ToListAsync();

            return Json(medicos);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerHorarios(int medicoId, DateTime fecha)
        {
            var citasOcupadas = await _context.Citas
                .Where(c => c.MedicoId == medicoId && c.FechaHora.Date == fecha.Date && c.Estado != EstadoCita.Cancelada)
                .Select(c => c.FechaHora.TimeOfDay)
                .ToListAsync();

            var horarios = new List<TimeSpan>();
            var inicio = new TimeSpan(8, 0, 0);
            var fin = new TimeSpan(17, 0, 0);

            for (var h = inicio; h < fin; h = h.Add(TimeSpan.FromMinutes(30)))
            {
                if (!citasOcupadas.Contains(h))
                    horarios.Add(h);
            }

            return Json(horarios);
        }

        private static CitaViewModel MapToViewModel(Cita c) => new()
        {
            Id = c.Id,
            MedicoNombre = c.Medico?.Usuario?.NombreCompleto ?? "",
            FechaHora = c.FechaHora,
            TipoCita = c.TipoCita.ToString(),
            Estado = c.Estado.ToString(),
            Motivo = c.Motivo
        };
    }
}

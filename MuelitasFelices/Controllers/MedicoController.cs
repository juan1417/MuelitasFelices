using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuelitasFelices.Data;
using MuelitasFelices.Models.Entities;
using MuelitasFelices.Models.ViewModels;

namespace MuelitasFelices.Controllers
{
    [Authorize(Roles = "Medico")]
    public class MedicoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Usuario> _userManager;

        public MedicoController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.UsuarioId == user.Id);
            if (medico == null) return NotFound();

            var hoy = DateTime.UtcNow.Date;
            var citasHoy = await _context.Citas
                .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                .Where(c => c.MedicoId == medico.Id && c.FechaHora.Date == hoy && c.Estado != EstadoCita.Cancelada)
                .OrderBy(c => c.FechaHora)
                .ToListAsync();

            var totalPacientes = await _context.Citas
                .Where(c => c.MedicoId == medico.Id)
                .Select(c => c.PacienteId)
                .Distinct()
                .CountAsync();

            var model = new DashboardMedicoViewModel
            {
                CitasHoy = citasHoy.Count,
                TotalPacientes = totalPacientes,
                CitasPendientes = await _context.Citas
                    .CountAsync(c => c.MedicoId == medico.Id && c.Estado == EstadoCita.Pendiente),
                CitasDelDia = citasHoy.Select(c => new CitaViewModel
                {
                    Id = c.Id,
                    PacienteId = c.PacienteId,
                    PacienteNombre = c.Paciente.Usuario.NombreCompleto,
                    FechaHora = c.FechaHora,
                    TipoCita = c.TipoCita.ToString(),
                    Estado = c.Estado.ToString(),
                    Motivo = c.Motivo
                }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> MisCitas()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.UsuarioId == user.Id);
            if (medico == null) return NotFound();

            var citas = await _context.Citas
                .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                .Where(c => c.MedicoId == medico.Id)
                .OrderByDescending(c => c.FechaHora)
                .ToListAsync();

            var model = new MisCitasViewModel
            {
                ProximasCitas = citas
                    .Where(c => c.FechaHora >= DateTime.UtcNow && c.Estado != EstadoCita.Cancelada)
                    .Select(c => new CitaViewModel
                    {
                        Id = c.Id,
                        PacienteId = c.PacienteId,
                        PacienteNombre = c.Paciente.Usuario.NombreCompleto,
                        FechaHora = c.FechaHora,
                        TipoCita = c.TipoCita.ToString(),
                        Estado = c.Estado.ToString(),
                        Motivo = c.Motivo
                    }).ToList(),
                HistorialCitas = citas
                    .Where(c => c.FechaHora < DateTime.UtcNow || c.Estado == EstadoCita.Cancelada)
                    .Select(c => new CitaViewModel
                    {
                        Id = c.Id,
                        PacienteId = c.PacienteId,
                        PacienteNombre = c.Paciente.Usuario.NombreCompleto,
                        FechaHora = c.FechaHora,
                        TipoCita = c.TipoCita.ToString(),
                        Estado = c.Estado.ToString(),
                        Motivo = c.Motivo
                    }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Pacientes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.UsuarioId == user.Id);
            if (medico == null) return NotFound();

            var pacientesIds = await _context.Citas
                .Where(c => c.MedicoId == medico.Id)
                .Select(c => c.PacienteId)
                .Distinct()
                .ToListAsync();

            var pacientes = await _context.Pacientes
                .Include(p => p.Usuario)
                .Where(p => pacientesIds.Contains(p.Id))
                .ToListAsync();

            var model = pacientes.Select(p => new PacienteHistorialViewModel
            {
                PacienteId = p.Id,
                PacienteNombre = p.Usuario.NombreCompleto,
                Email = p.Usuario.Email ?? "",
                Telefono = p.Usuario.PhoneNumber ?? "",
                FechaNacimiento = p.FechaNacimiento,
                Direccion = p.Direccion
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Historial(int pacienteId, int? citaId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.UsuarioId == user.Id);
            if (medico == null) return NotFound();

            if (citaId.HasValue && pacienteId == 0)
            {
                var citaInfo = await _context.Citas.FindAsync(citaId.Value);
                if (citaInfo != null) pacienteId = citaInfo.PacienteId;
            }

            var paciente = await _context.Pacientes.Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == pacienteId);
            if (paciente == null) return NotFound();

            if (citaId == null)
            {
                var citasPendientes = await _context.Citas
                    .Include(c => c.HistorialMedico)
                    .Where(c => c.PacienteId == pacienteId && c.MedicoId == medico.Id
                        && c.Estado == EstadoCita.Completada && c.HistorialMedico == null)
                    .OrderByDescending(c => c.FechaHora)
                    .ToListAsync();

                ViewBag.CitasPendientes = citasPendientes;
                ViewBag.PacienteNombre = paciente.Usuario.NombreCompleto;
                ViewBag.PacienteId = pacienteId;

                var historialExistente = await _context.HistorialMedicos
                    .Include(h => h.Medico).ThenInclude(m => m.Usuario)
                    .Include(h => h.Cita)
                    .Where(h => h.PacienteId == pacienteId && h.MedicoId == medico.Id)
                    .OrderByDescending(h => h.Fecha)
                    .ToListAsync();

                return View("HistorialPaciente", historialExistente);
            }

            var cita = await _context.Citas.FindAsync(citaId.Value);
            if (cita == null) return NotFound();

            var historialExist = await _context.HistorialMedicos
                .FirstOrDefaultAsync(h => h.CitaId == citaId.Value);

            var model = new HistorialViewModel
            {
                PacienteId = pacienteId,
                PacienteNombre = paciente.Usuario.NombreCompleto,
                CitaId = citaId.Value,
                FechaCita = cita.FechaHora,
                MotivoCita = cita.Motivo,
                HistorialId = historialExist?.Id,
                Diagnostico = historialExist?.Diagnostico ?? "",
                Tratamiento = historialExist?.Tratamiento ?? "",
                Observaciones = historialExist?.Observaciones ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarHistorial(HistorialViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Historial", model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.UsuarioId == user.Id);
            if (medico == null) return NotFound();

            var historial = await _context.HistorialMedicos
                .FirstOrDefaultAsync(h => h.CitaId == model.CitaId);

            if (historial == null)
            {
                historial = new HistorialMedico
                {
                    PacienteId = model.PacienteId,
                    MedicoId = medico.Id,
                    CitaId = model.CitaId,
                    Fecha = DateTime.UtcNow,
                    Diagnostico = model.Diagnostico,
                    Tratamiento = model.Tratamiento,
                    Observaciones = model.Observaciones
                };
                _context.HistorialMedicos.Add(historial);
            }
            else
            {
                historial.Diagnostico = model.Diagnostico;
                historial.Tratamiento = model.Tratamiento;
                historial.Observaciones = model.Observaciones;
                historial.Fecha = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Historial guardado correctamente.";
            return RedirectToAction(nameof(Historial), new { pacienteId = model.PacienteId, citaId = model.CitaId });
        }
    }
}

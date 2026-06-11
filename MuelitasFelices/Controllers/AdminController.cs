using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuelitasFelices.Data;
using MuelitasFelices.Models.Entities;
using MuelitasFelices.Models.ViewModels;

namespace MuelitasFelices.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardAdminViewModel
            {
                TotalUsuarios = await _context.Users.CountAsync(),
                TotalMedicos = await _context.Medicos.CountAsync(),
                TotalPacientes = await _context.Pacientes.CountAsync(),
                TotalCitasPendientes = await _context.Citas.CountAsync(c => c.Estado == EstadoCita.Pendiente),
                TotalCitasHoy = await _context.Citas.CountAsync(c => c.FechaHora.Date == DateTime.UtcNow.Date),
                CitasRecientes = await _context.Citas
                    .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                    .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                    .OrderByDescending(c => c.FechaCreacion)
                    .Take(10)
                    .Select(c => new CitaAdminViewModel
                    {
                        Id = c.Id,
                        PacienteNombre = c.Paciente.Usuario.NombreCompleto,
                        MedicoNombre = c.Medico.Usuario.NombreCompleto,
                        FechaHora = c.FechaHora,
                        TipoCita = c.TipoCita.ToString(),
                        Estado = c.Estado.ToString(),
                        Motivo = c.Motivo
                    }).ToListAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _context.Users.ToListAsync();
            var model = new List<UsuarioAdminViewModel>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                model.Add(new UsuarioAdminViewModel
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email ?? "",
                    Telefono = u.PhoneNumber ?? "",
                    Rol = roles.FirstOrDefault() ?? "Ninguno",
                    FechaRegistro = u.FechaRegistro
                });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditarUsuario(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var rolesDisponibles = await _roleManager.Roles.Select(r => r.Name ?? "").ToListAsync();

            var model = new EditarUsuarioViewModel
            {
                Id = user.Id,
                NombreCompleto = user.NombreCompleto,
                Email = user.Email ?? "",
                Telefono = user.PhoneNumber ?? "",
                Rol = roles.FirstOrDefault() ?? "Paciente",
                RolesDisponibles = rolesDisponibles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(EditarUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RolesDisponibles = await _roleManager.Roles.Select(r => r.Name ?? "").ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.NombreCompleto = model.NombreCompleto;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.Telefono;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Rol);

            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        public async Task<IActionResult> Medicos()
        {
            var medicos = await _context.Medicos
                .Include(m => m.Usuario)
                .Include(m => m.MedicoEspecialidades).ThenInclude(me => me.Especialidad)
                .ToListAsync();

            var model = medicos.Select(m => new MedicoAdminViewModel
            {
                Id = m.Id,
                UsuarioId = m.UsuarioId,
                NombreCompleto = m.Usuario.NombreCompleto,
                Email = m.Usuario.Email ?? "",
                NumeroLicencia = m.NumeroLicencia,
                Especialidades = m.MedicoEspecialidades.Select(me => me.Especialidad.Nombre).ToList()
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AsignarEspecialidad(int medicoId)
        {
            var medico = await _context.Medicos.Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == medicoId);
            if (medico == null) return NotFound();

            var especialidadesAsignadas = await _context.MedicoEspecialidades
                .Where(me => me.MedicoId == medicoId)
                .Select(me => me.EspecialidadId)
                .ToListAsync();

            ViewBag.MedicoNombre = medico.Usuario.NombreCompleto;
            ViewBag.MedicoId = medicoId;
            ViewBag.EspecialidadesAsignadas = especialidadesAsignadas;

            var especialidades = await _context.Especialidades.ToListAsync();
            return View(especialidades);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarEspecialidad(int medicoId, List<int> especialidadIds)
        {
            var existing = await _context.MedicoEspecialidades
                .Where(me => me.MedicoId == medicoId)
                .ToListAsync();
            _context.MedicoEspecialidades.RemoveRange(existing);

            foreach (var espId in especialidadIds)
            {
                _context.MedicoEspecialidades.Add(new MedicoEspecialidad
                {
                    MedicoId = medicoId,
                    EspecialidadId = espId
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Especialidades asignadas correctamente.";
            return RedirectToAction(nameof(Medicos));
        }

        [HttpGet]
        public async Task<IActionResult> CrearMedico()
        {
            ViewBag.Especialidades = await _context.Especialidades.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearMedico(string nombreCompleto, string email, string password, string telefono, string numeroLicencia, List<int> especialidadIds)
        {
            var user = new Usuario
            {
                UserName = email,
                Email = email,
                NombreCompleto = nombreCompleto,
                PhoneNumber = telefono,
                FechaRegistro = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Medico");

                var medico = new Medico
                {
                    UsuarioId = user.Id,
                    NumeroLicencia = numeroLicencia
                };
                _context.Medicos.Add(medico);
                await _context.SaveChangesAsync();

                foreach (var espId in especialidadIds)
                {
                    _context.MedicoEspecialidades.Add(new MedicoEspecialidad
                    {
                        MedicoId = medico.Id,
                        EspecialidadId = espId
                    });
                }
                await _context.SaveChangesAsync();

                TempData["Success"] = "Médico creado correctamente.";
                return RedirectToAction(nameof(Medicos));
            }

            ViewBag.Especialidades = await _context.Especialidades.ToListAsync();
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View();
        }

        public async Task<IActionResult> Citas()
        {
            var citas = await _context.Citas
                .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                .OrderByDescending(c => c.FechaHora)
                .Select(c => new CitaAdminViewModel
                {
                    Id = c.Id,
                    PacienteNombre = c.Paciente.Usuario.NombreCompleto,
                    MedicoNombre = c.Medico.Usuario.NombreCompleto,
                    FechaHora = c.FechaHora,
                    TipoCita = c.TipoCita.ToString(),
                    Estado = c.Estado.ToString(),
                    Motivo = c.Motivo
                }).ToListAsync();

            return View(citas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoCita(int id, string nuevoEstado)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            if (Enum.TryParse<EstadoCita>(nuevoEstado, out var estado))
            {
                cita.Estado = estado;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Estado de cita actualizado.";
            }

            return RedirectToAction(nameof(Citas));
        }

        [HttpGet]
        public async Task<IActionResult> CrearCitaManual()
        {
            ViewBag.Pacientes = await _context.Pacientes.Include(p => p.Usuario).ToListAsync();
            ViewBag.Medicos = await _context.Medicos.Include(m => m.Usuario)
                .Include(m => m.MedicoEspecialidades).ThenInclude(me => me.Especialidad).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCitaManual(int pacienteId, int medicoId, DateTime fecha, string hora, string tipoCita, string motivo)
        {
            if (!TimeSpan.TryParse(hora, out var horaSpan))
            {
                TempData["Error"] = "Hora inválida";
                return RedirectToAction(nameof(CrearCitaManual));
            }

            if (!Enum.TryParse<TipoCita>(tipoCita, out var tipo))
                tipo = TipoCita.General;

            var cita = new Cita
            {
                PacienteId = pacienteId,
                MedicoId = medicoId,
                FechaHora = fecha.Date + horaSpan,
                TipoCita = tipo,
                Motivo = motivo,
                Estado = EstadoCita.Confirmada
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cita creada manualmente.";
            return RedirectToAction(nameof(Citas));
        }

        public async Task<IActionResult> Especialidades()
        {
            var especialidades = await _context.Especialidades.ToListAsync();
            return View(especialidades);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEspecialidad(string nombre, string descripcion)
        {
            _context.Especialidades.Add(new Especialidad
            {
                Nombre = nombre,
                Descripcion = descripcion
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Especialidad creada.";
            return RedirectToAction(nameof(Especialidades));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEspecialidad(int id)
        {
            var esp = await _context.Especialidades.FindAsync(id);
            if (esp != null)
            {
                _context.Especialidades.Remove(esp);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Especialidad eliminada.";
            }
            return RedirectToAction(nameof(Especialidades));
        }
    }
}

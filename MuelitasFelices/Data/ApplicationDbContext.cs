using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MuelitasFelices.Models.Entities;

namespace MuelitasFelices.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Especialidad> Especialidades { get; set; }
        public DbSet<Medico> Medicos { get; set; }
        public DbSet<MedicoEspecialidad> MedicoEspecialidades { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<HistorialMedico> HistorialMedicos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MedicoEspecialidad>()
                .HasKey(me => new { me.MedicoId, me.EspecialidadId });

            builder.Entity<MedicoEspecialidad>()
                .HasOne(me => me.Medico)
                .WithMany(m => m.MedicoEspecialidades)
                .HasForeignKey(me => me.MedicoId);

            builder.Entity<MedicoEspecialidad>()
                .HasOne(me => me.Especialidad)
                .WithMany(e => e.MedicoEspecialidades)
                .HasForeignKey(me => me.EspecialidadId);

            builder.Entity<Medico>()
                .HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Paciente>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Cita>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.Citas)
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Cita>()
                .HasOne(c => c.Medico)
                .WithMany(m => m.Citas)
                .HasForeignKey(c => c.MedicoId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Cita>()
                .HasOne(c => c.CitaAnterior)
                .WithMany(c => c.CitasDerivadas)
                .HasForeignKey(c => c.CitaAnteriorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<HistorialMedico>()
                .HasOne(h => h.Paciente)
                .WithMany(p => p.HistorialMedicos)
                .HasForeignKey(h => h.PacienteId);

            builder.Entity<HistorialMedico>()
                .HasOne(h => h.Medico)
                .WithMany(m => m.HistorialMedicos)
                .HasForeignKey(h => h.MedicoId);

            builder.Entity<HistorialMedico>()
                .HasOne(h => h.Cita)
                .WithOne(c => c.HistorialMedico)
                .HasForeignKey<HistorialMedico>(h => h.CitaId);

            builder.Entity<Usuario>(entity =>
            {
                entity.Property(u => u.NombreCompleto).HasMaxLength(200);
            });
        }
    }
}

using Microsoft.AspNetCore.Identity;
using MuelitasFelices.Models.Entities;

namespace MuelitasFelices.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            context.Database.EnsureCreated();

            string[] roles = { "Admin", "Medico", "Paciente" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (!context.Especialidades.Any())
            {
                var especialidades = new List<Especialidad>
                {
                    new() { Nombre = "Odontología General", Descripcion = "Atención dental primaria y preventiva" },
                    new() { Nombre = "Ortodoncia", Descripcion = "Corrección de la posición de los dientes" },
                    new() { Nombre = "Endodoncia", Descripcion = "Tratamiento de conducto (nervio)" },
                    new() { Nombre = "Periodoncia", Descripcion = "Tratamiento de encías y soporte dental" },
                    new() { Nombre = "Cirugía Oral", Descripcion = "Extracciones y cirugía maxilofacial" },
                    new() { Nombre = "Odontopediatría", Descripcion = "Atención dental para niños" },
                    new() { Nombre = "Implantes", Descripcion = "Colocación de implantes dentales" },
                    new() { Nombre = "Estética Dental", Descripcion = "Blanqueamiento y carillas" }
                };
                context.Especialidades.AddRange(especialidades);
                await context.SaveChangesAsync();
            }

            if (await userManager.FindByEmailAsync("admin@muelitas.com") == null)
            {
                var adminUser = new Usuario
                {
                    UserName = "admin@muelitas.com",
                    Email = "admin@muelitas.com",
                    NombreCompleto = "Administrador",
                    PhoneNumber = "0000000000",
                    FechaRegistro = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}

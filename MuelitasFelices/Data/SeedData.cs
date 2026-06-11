using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            if (!context.Especialidades.Any())
            {
                context.Especialidades.AddRange(new List<Especialidad>
                {
                    new() { Nombre = "Odontología General", Descripcion = "Atención dental primaria y preventiva" },
                    new() { Nombre = "Ortodoncia", Descripcion = "Corrección de la posición de los dientes" },
                    new() { Nombre = "Endodoncia", Descripcion = "Tratamiento de conducto (nervio)" },
                    new() { Nombre = "Periodoncia", Descripcion = "Tratamiento de encías y soporte dental" },
                    new() { Nombre = "Cirugía Oral", Descripcion = "Extracciones y cirugía maxilofacial" },
                    new() { Nombre = "Odontopediatría", Descripcion = "Atención dental para niños" },
                    new() { Nombre = "Implantes", Descripcion = "Colocación de implantes dentales" },
                    new() { Nombre = "Estética Dental", Descripcion = "Blanqueamiento y carillas" }
                });
                await context.SaveChangesAsync();
            }

            if (await userManager.FindByEmailAsync("admin@muelitas.com") == null)
            {
                var adminUser = new Usuario
                {
                    UserName = "admin@muelitas.com", Email = "admin@muelitas.com",
                    NombreCompleto = "Administrador del Sistema", PhoneNumber = "809-555-0000",
                    FechaRegistro = DateTime.UtcNow.AddDays(-90)
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            if (context.Pacientes.Any()) return;

            var especialidades = await context.Especialidades.ToListAsync();
            var espOdontGeneral = especialidades.First(e => e.Nombre == "Odontología General");
            var espOrtodoncia = especialidades.First(e => e.Nombre == "Ortodoncia");
            var espEndodoncia = especialidades.First(e => e.Nombre == "Endodoncia");
            var espPeriodoncia = especialidades.First(e => e.Nombre == "Periodoncia");
            var espCirugia = especialidades.First(e => e.Nombre == "Cirugía Oral");
            var espPediatria = especialidades.First(e => e.Nombre == "Odontopediatría");
            var espImplantes = especialidades.First(e => e.Nombre == "Implantes");
            var espEstetica = especialidades.First(e => e.Nombre == "Estética Dental");

            var medicosData = new[]
            {
                (nombre: "Dr. Carlos López", email: "carlos.lopez@muelitas.com", licencia: "LIC-001", esp: new[] { espOrtodoncia, espEstetica }),
                (nombre: "Dra. María García", email: "maria.garcia@muelitas.com", licencia: "LIC-002", esp: new[] { espEndodoncia, espOdontGeneral }),
                (nombre: "Dr. Juan Pérez", email: "juan.perez@muelitas.com", licencia: "LIC-003", esp: new[] { espCirugia, espImplantes }),
                (nombre: "Dra. Ana Martínez", email: "ana.martinez@muelitas.com", licencia: "LIC-004", esp: new[] { espPeriodoncia, espOdontGeneral }),
                (nombre: "Dr. Roberto Sánchez", email: "roberto.sanchez@muelitas.com", licencia: "LIC-005", esp: new[] { espPediatria }),
                (nombre: "Dra. Laura Torres", email: "laura.torres@muelitas.com", licencia: "LIC-006", esp: new[] { espEstetica, espOrtodoncia })
            };

            var medicos = new List<Medico>();
            foreach (var (nombre, email, licencia, esp) in medicosData)
            {
                var user = new Usuario
                {
                    UserName = email, Email = email, NombreCompleto = nombre,
                    PhoneNumber = $"809-555-{new Random().Next(1000, 9999)}",
                    FechaRegistro = DateTime.UtcNow.AddDays(-85)
                };
                await userManager.CreateAsync(user, "Medico123!");
                await userManager.AddToRoleAsync(user, "Medico");

                var medico = new Medico { UsuarioId = user.Id, NumeroLicencia = licencia };
                context.Medicos.Add(medico);
                await context.SaveChangesAsync();

                foreach (var e in esp)
                    context.MedicoEspecialidades.Add(new MedicoEspecialidad { MedicoId = medico.Id, EspecialidadId = e.Id });
                await context.SaveChangesAsync();
                medicos.Add(medico);
            }

            var pacientesData = new[]
            {
                ("Pedro Rodríguez", "pedro.rodriguez@email.com", "809-555-1001", new DateTime(1985, 3, 15), "Calle Principal 123, Santo Domingo"),
                ("María Fernández", "maria.fernandez@email.com", "809-555-1002", new DateTime(1990, 7, 22), "Av. Independencia 456, Santo Domingo"),
                ("José Castillo", "jose.castillo@email.com", "809-555-1003", new DateTime(1978, 11, 8), "Calle Duarte 789, Santiago"),
                ("Ana Reyes", "ana.reyes@email.com", "809-555-1004", new DateTime(2000, 2, 14), "Calle Colón 321, La Vega"),
                ("Luis Martínez", "luis.martinez@email.com", "809-555-1005", new DateTime(1995, 9, 30), "Av. España 654, San Pedro"),
                ("Carmen Díaz", "carmen.diaz@email.com", "809-555-1006", new DateTime(1982, 5, 10), "Calle El Sol 987, Santo Domingo"),
                ("Carlos Hernández", "carlos.hernandez@email.com", "809-555-1007", new DateTime(1975, 12, 3), "Av. Churchill 147, Santo Domingo"),
                ("Laura Sánchez", "laura.sanchez@email.com", "809-555-1008", new DateTime(1998, 8, 25), "Calle Las Palmas 258, Santiago"),
                ("Miguel Torres", "miguel.torres@email.com", "809-555-1009", new DateTime(1988, 4, 18), "Av. 27 de Febrero 369, Santo Domingo"),
                ("Sofía Ramírez", "sofia.ramirez@email.com", "809-555-1010", new DateTime(2002, 1, 5), "Calle Los Robles 159, La Romana"),
                ("Diego Morales", "diego.morales@email.com", "809-555-1011", new DateTime(1992, 6, 12), "Av. San Martín 753, Santiago"),
                ("Valentina Ortiz", "valentina.ortiz@email.com", "809-555-1012", new DateTime(1986, 10, 28), "Calle del Sol 951, Santo Domingo"),
                ("Andrés Vega", "andres.vega@email.com", "809-555-1013", new DateTime(1979, 3, 7), "Av. Sarasota 357, Santo Domingo"),
                ("Gabriela Núñez", "gabriela.nunez@email.com", "809-555-1014", new DateTime(1996, 7, 19), "Calle Benito Juárez 852, Higüey"),
                ("Fernando Peña", "fernando.pena@email.com", "809-555-1015", new DateTime(1983, 11, 2), "Av. Máximo Gómez 468, Santo Domingo"),
                ("Isabel Guzmán", "isabel.guzman@email.com", "809-555-1016", new DateTime(1999, 4, 9), "Calle El Conde 741, Santo Domingo"),
                ("Ricardo Flores", "ricardo.flores@email.com", "809-555-1017", new DateTime(1980, 8, 21), "Av. Bolívar 236, Santiago"),
                ("Patricia Medina", "patricia.medina@email.com", "809-555-1018", new DateTime(1993, 12, 15), "Calle Hostos 874, San Cristóbal"),
                ("Jorge Rivas", "jorge.rivas@email.com", "809-555-1019", new DateTime(1987, 5, 30), "Av. Rómulo Betancourt 543, Santo Domingo"),
                ("Daniela Campos", "daniela.campos@email.com", "809-555-1020", new DateTime(2001, 9, 11), "Calle Núñez de Cáceres 312, Santo Domingo")
            };

            var pacientes = new List<Paciente>();
            foreach (var (nombre, email, telefono, fechaNac, direccion) in pacientesData)
            {
                var user = new Usuario
                {
                    UserName = email, Email = email, NombreCompleto = nombre,
                    PhoneNumber = telefono, FechaRegistro = DateTime.UtcNow.AddDays(-80)
                };
                await userManager.CreateAsync(user, "Paciente123!");
                await userManager.AddToRoleAsync(user, "Paciente");

                var paciente = new Paciente { UsuarioId = user.Id, FechaNacimiento = fechaNac, Direccion = direccion };
                context.Pacientes.Add(paciente);
                await context.SaveChangesAsync();
                pacientes.Add(paciente);
            }

            var rng = new Random(42);
            var now = DateTime.UtcNow;
            var citas = new List<Cita>();

            void AddCita(Paciente p, Medico m, DateTime fecha, TipoCita tipo, EstadoCita estado, string motivo, int? anteriorId = null)
            {
                citas.Add(new Cita
                {
                    PacienteId = p.Id, MedicoId = m.Id, FechaHora = fecha,
                    TipoCita = tipo, Estado = estado, Motivo = motivo,
                    CitaAnteriorId = anteriorId, FechaCreacion = fecha.AddDays(-rng.Next(1, 14))
                });
            }

            AddCita(pacientes[0], medicos[0], now.Date.AddDays(-45).AddHours(10), TipoCita.General, EstadoCita.Completada, "Dolor en muela del juicio");
            AddCita(pacientes[0], medicos[0], now.Date.AddDays(-14).AddHours(11), TipoCita.PorHistorial, EstadoCita.Completada, "Revisión post-extracción", 1);
            AddCita(pacientes[1], medicos[1], now.Date.AddDays(-60).AddHours(9), TipoCita.General, EstadoCita.Completada, "Dolor de muelas intenso");
            AddCita(pacientes[1], medicos[1], now.Date.AddDays(-30).AddHours(10), TipoCita.PorHistorial, EstadoCita.Completada, "Control de endodoncia", 3);
            AddCita(pacientes[2], medicos[2], now.Date.AddDays(-20).AddHours(14), TipoCita.Remitido, EstadoCita.Completada, "Remitido por Dr. Martínez para cirugía");
            AddCita(pacientes[3], medicos[3], now.Date.AddDays(-50).AddHours(8), TipoCita.General, EstadoCita.Completada, "Limpieza dental general");
            AddCita(pacientes[4], medicos[4], now.Date.AddDays(-35).AddHours(15), TipoCita.General, EstadoCita.Completada, "Revisión dental niño");
            AddCita(pacientes[5], medicos[5], now.Date.AddDays(-25).AddHours(11), TipoCita.General, EstadoCita.Completada, "Blanqueamiento dental");
            AddCita(pacientes[6], medicos[0], now.Date.AddDays(-40).AddHours(9), TipoCita.General, EstadoCita.Completada, "Consulta por brackets");
            AddCita(pacientes[7], medicos[1], now.Date.AddDays(-55).AddHours(10), TipoCita.General, EstadoCita.Completada, "Dolor de encías");
            AddCita(pacientes[8], medicos[2], now.Date.AddDays(-10).AddHours(16), TipoCita.Remitido, EstadoCita.Completada, "Evaluación para implante");
            AddCita(pacientes[9], medicos[3], now.Date.AddDays(-18).AddHours(8), TipoCita.General, EstadoCita.Completada, "Sangrado de encías");
            AddCita(pacientes[10], medicos[4], now.Date.AddDays(-5).AddHours(14), TipoCita.General, EstadoCita.Completada, "Revisión pediatrica");
            AddCita(pacientes[11], medicos[5], now.Date.AddDays(-8).AddHours(10), TipoCita.PorHistorial, EstadoCita.Completada, "Control de blanqueamiento", 8);
            AddCita(pacientes[12], medicos[0], now.Date.AddDays(-3).AddHours(9), TipoCita.General, EstadoCita.Completada, "Colocación de brackets");

            AddCita(pacientes[0], medicos[0], now.Date.AddDays(3).AddHours(10), TipoCita.PorHistorial, EstadoCita.Confirmada, "Control post-operatorio", 1);
            AddCita(pacientes[2], medicos[2], now.Date.AddDays(5).AddHours(14), TipoCita.PorHistorial, EstadoCita.Confirmada, "Revisión de implante", 5);
            AddCita(pacientes[13], medicos[1], now.Date.AddDays(7).AddHours(9), TipoCita.General, EstadoCita.Pendiente, "Dolor de muelas");
            AddCita(pacientes[14], medicos[4], now.Date.AddDays(2).AddHours(15), TipoCita.General, EstadoCita.Pendiente, "Revisión hija de 8 años");
            AddCita(pacientes[15], medicos[3], now.Date.AddDays(10).AddHours(11), TipoCita.General, EstadoCita.Pendiente, "Limpieza dental profunda");
            AddCita(pacientes[16], medicos[5], now.Date.AddDays(14).AddHours(10), TipoCita.General, EstadoCita.Pendiente, "Consulta para carillas");
            AddCita(pacientes[17], medicos[0], now.Date.AddDays(21).AddHours(8), TipoCita.General, EstadoCita.Pendiente, "Evaluación ortodóncica");
            AddCita(pacientes[18], medicos[2], now.Date.AddDays(4).AddHours(16), TipoCita.Remitido, EstadoCita.Confirmada, "Remitido por Dra. García para extracción");
            AddCita(pacientes[19], medicos[1], now.Date.AddDays(12).AddHours(14), TipoCita.General, EstadoCita.Pendiente, "Revisión de caries");

            AddCita(pacientes[3], medicos[3], now.Date.AddDays(-12).AddHours(9), TipoCita.General, EstadoCita.Cancelada, "Cambio de fecha por paciente");
            AddCita(pacientes[7], medicos[1], now.Date.AddDays(-7).AddHours(11), TipoCita.PorHistorial, EstadoCita.Cancelada, "Cancelado por emergencia");
            AddCita(pacientes[11], medicos[5], now.Date.AddDays(-2).AddHours(10), TipoCita.General, EstadoCita.Cancelada, "Paciente no asistió");
            AddCita(pacientes[5], medicos[0], now.Date.AddDays(1).AddHours(15), TipoCita.Remitido, EstadoCita.Cancelada, "Cancelado por médico");

            context.Citas.AddRange(citas);
            await context.SaveChangesAsync();

            var completedCitas = citas.Where(c => c.Estado == EstadoCita.Completada).ToList();
            var historiales = new List<HistorialMedico>();

            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[0].Id, MedicoId = medicos[0].Id, CitaId = completedCitas[0].Id, Fecha = completedCitas[0].FechaHora,
                Diagnostico = "<p><strong>Diagnóstico:</strong> Cordal inferior derecho impactado (pieza 48) con pericoronitis leve.</p><p>Se observa inflamación y dolor en la zona. Radiografía muestra impactación mesioangular.</p>",
                Tratamiento = "<p><strong>Tratamiento realizado:</strong> Extracción quirúrgica del cordal inferior derecho bajo anestesia local.</p><ul><li>Anestesia troncular con lidocaína al 2%</li><li>Incisión y colgajo mucoperióstico</li><li>Odontosección y extracción por piezas</li><li>Sutura con seda 3-0</li></ul>",
                Observaciones = "<p>Paciente toleró bien el procedimiento. Se recetó amoxicilina 500mg cada 8h por 7 días e ibuprofeno 600mg cada 8h. Cita de control en 7 días.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[0].Id, MedicoId = medicos[0].Id, CitaId = completedCitas[1].Id, Fecha = completedCitas[1].FechaHora,
                Diagnostico = "<p>Herida quirúrgica en proceso de cicatrización. Tejido de granulación presente. Signos vitales normales.</p>",
                Tratamiento = "<p>Retiro de puntos de sutura. Limpieza de la zona con clorhexidina al 0.12%.</p><p>Se indica continuar con enjuague de clorhexidina 2 veces al día por 5 días más.</p>",
                Observaciones = "<p>Evolución favorable. Cicatrización dentro de lo esperado. Alta médica. Próxima cita en 6 meses para control de rutina.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[1].Id, MedicoId = medicos[1].Id, CitaId = completedCitas[2].Id, Fecha = completedCitas[2].FechaHora,
                Diagnostico = "<p><strong>Diagnóstico:</strong> Caries profunda en pieza 36 con afectación pulpar irreversible. Pulpitis irreversible sintomática.</p><p>Radiografía muestra caries que alcanza la cámara pulpar.</p>",
                Tratamiento = "<p><strong>Procedimiento:</strong> Tratamiento de conducto (endodoncia) en pieza 36.</p><ul><li>Aislamiento absoluto con dique de goma</li><li>Apertura cameral y localización de conductos</li><li>Instrumentación con limas rotatorias ProTaper</li><li>Irrigación con hipoclorito de sodio al 5.25%</li><li>Obturación con conos de gutapercha y sellador AH Plus</li></ul>",
                Observaciones = "<p>Se realizó en una sola sesión. Paciente reporta dolor leve post-operatorio. Se recetó ibuprofeno 600mg cada 8h si es necesario. Cita en 2 semanas para corona definitiva.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[1].Id, MedicoId = medicos[1].Id, CitaId = completedCitas[3].Id, Fecha = completedCitas[3].FechaHora,
                Diagnostico = "<p>Control post-endodoncia. Pieza 36 asintomática. Radiografía de control muestra obturación de conductos adecuada, sin lesión periapical.</p>",
                Tratamiento = "<p>Colocación de corona de zirconia sobre pieza 36. Preparación del muñón, impresión digital y cementación con ionómero de vidrio.</p>",
                Observaciones = "<p>Corona colocada con ajuste oclusal adecuado. Paciente satisfecho. Alta del tratamiento de endodoncia.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[2].Id, MedicoId = medicos[2].Id, CitaId = completedCitas[4].Id, Fecha = completedCitas[4].FechaHora,
                Diagnostico = "<p><strong>Diagnóstico:</strong> Paciente remitido por Dr. Martínez. Edentulismo parcial en sector anterosuperior. Pérdida de pieza 21 por traumatismo.</p><p>Radiografía muestra suficiente hueso residual para implante.</p>",
                Tratamiento = "<p>Evaluación pre-quirúrgica completa. TAC de haz cónico muestra densidad ósea tipo II. Se planifica colocación de implante dental en posición 21.</p>",
                Observaciones = "<p>Pendiente de cirugía de implante programada. Se recetó enjuague de clorhexidina pre-operatorio. Próxima cita para colocación de implante.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[3].Id, MedicoId = medicos[3].Id, CitaId = completedCitas[5].Id, Fecha = completedCitas[5].FechaHora,
                Diagnostico = "<p>Paciente con acumulación de sarro y placa bacteriana generalizada. Gingivitis leve asociada a mala higiene oral. Índice de placa: 45%.</p>",
                Tratamiento = "<p>Profilaxis dental completa: ultrasonido, raspaje y alisado radicular, pulido con pasta profiláctica y aplicación de flúor.</p>",
                Observaciones = "<p>Se educó al paciente sobre técnica de cepillado (Bass modificado) y uso de hilo dental. Se recomienda cepillo eléctrico. Próxima cita en 6 meses.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[4].Id, MedicoId = medicos[4].Id, CitaId = completedCitas[6].Id, Fecha = completedCitas[6].FechaHora,
                Diagnostico = "<p><strong>Diagnóstico:</strong> Paciente pediátrico de 6 años. Caries interproximal en pieza temporal 55. Mordida cruzada posterior derecha leve.</p>",
                Tratamiento = "<p>Obturación con resina compuesta en pieza 55. Aplicación de flúor barniz en todas las piezas. Remisión a ortodoncista para evaluación de mordida cruzada.</p>",
                Observaciones = "<p>Niño cooperador durante el tratamiento. Se indicó a los padres reducir consumo de azúcares y mejorar higiene. Próxima cita en 3 meses.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[5].Id, MedicoId = medicos[5].Id, CitaId = completedCitas[7].Id, Fecha = completedCitas[7].FechaHora,
                Diagnostico = "<p>Pigmentación dental extrínseca por consumo de café y té. Tono dental actual A3 en escala Vita. Esmalte dental sano sin caries.</p>",
                Tratamiento = "<p><strong>Blanqueamiento dental ambulatorio:</strong> Férulas de blanqueamiento con peróxido de carbamida al 16%. Uso nocturno por 2 semanas.</p><p>Se tomaron impresiones digitales para fabricación de férulas. Entrega programada en 48 horas.</p>",
                Observaciones = "<p>Se explicó técnica de aplicación y posibles sensaciones de sensibilidad. Se recetó gel de flúor para usar en caso de sensibilidad. Cita de control en 15 días.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[6].Id, MedicoId = medicos[0].Id, CitaId = completedCitas[8].Id, Fecha = completedCitas[8].FechaHora,
                Diagnostico = "<p>Maloclusión clase II división 1. Apiñamiento severo en arcada superior e inferior. Línea media desviada 3mm a la derecha. Perfil convexo.</p>",
                Tratamiento = "<p>Toma de registros ortodóncicos: fotografías intraorales y extraorales, radiografía panorámica, cefalométrica lateral y modelos digitales.</p><p>Plan de tratamiento: extracción de premolares superiores e inferiores + tratamiento con brackets metálicos autoligables.</p>",
                Observaciones = "<p>Se explicó plan de tratamiento detallado y duración estimada de 24 meses. Próxima cita para colocación de separadores y posterior cementación de brackets.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[7].Id, MedicoId = medicos[1].Id, CitaId = completedCitas[9].Id, Fecha = completedCitas[9].FechaHora,
                Diagnostico = "<p>Periodontitis crónica moderada generalizada. Bolsas periodontales de 4-6mm en molares superiores e inferiores. Sangrado al sondaje generalizado. Movilidad grado I en piezas 26 y 36.</p>",
                Tratamiento = "<p>Raspaje y alisado radicular por cuadrantes bajo anestesia local. Se realizó cuadrante 1 (superior derecho) en esta sesión.</p>",
                Observaciones = "<p>Se recetó enjuague de clorhexidina al 0.12% cada 12h por 14 días. Se insiste en técnica de higiene. Próxima cita en 1 semana para continuar con cuadrante 2.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[8].Id, MedicoId = medicos[2].Id, CitaId = completedCitas[10].Id, Fecha = completedCitas[10].FechaHora,
                Diagnostico = "<p>Edentulismo unitario pieza 46 (extracción hace 6 meses por caries). Hueso residual suficiente. Anchura mesiodistal adecuada. Espacio interoclusal correcto.</p>",
                Tratamiento = "<p><strong>Cirugía de colocación de implante dental:</strong></p><ul><li>Anestesia local con articaína al 4%</li><li>Incisión crestal y elevación de colgajo</li><li>Osteotomía secuencial con fresas</li><li>Colocación de implante BioHorizons 4.5x10mm con torque de 35Ncm</li><li>Colocación de tornillo de cubierta y sutura</li></ul>",
                Observaciones = "<p>Paciente toleró bien el procedimiento. Radiografía post-operatoria muestra posición adecuada del implante. Se recetó antibiótico y analgésico. Próxima cita en 7 días para retiro de puntos. Segunda fase (corona) en 3 meses.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[9].Id, MedicoId = medicos[3].Id, CitaId = completedCitas[11].Id, Fecha = completedCitas[11].FechaHora,
                Diagnostico = "<p>Gingivitis generalizada asociada a placa bacteriana. Encías eritematosas y edematosas. Sangrado profuso al sondaje. Sin pérdida de inserción. Bolsas <3mm.</p>",
                Tratamiento = "<p>Destartraje supra y subgingival completo. Pulido coronal. Aplicación de clorhexidina en gel al 1% en zonas más inflamadas.</p>",
                Observaciones = "<p>Se realizó enseñanza de técnica de cepillado (Bass) y uso de irrigador dental. Se recomienda cepillo interdental. Próxima cita en 1 mes para reevaluación. Si persiste, considerar periodo de mantenimiento.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[10].Id, MedicoId = medicos[4].Id, CitaId = completedCitas[12].Id, Fecha = completedCitas[12].FechaHora,
                Diagnostico = "<p>Paciente pediátrico de 9 años. Caries oclusal en molar temporal 75. Mancha blanca activa en vestibular de incisivos superiores. Erupción de premolares en proceso.</p>",
                Tratamiento = "<p>Obturación preventiva con resina fluida en pieza 75. Aplicación de flúor barniz (Duraphat) en todas las piezas. Selladores de fosas y fisuras en primeros molares permanentes (piezas 16, 26, 36, 46).</p>",
                Observaciones = "<p>Paciente cooperador. Se reforzaron hábitos de higiene con los padres. Se recomienda reducir consumo de jugos azucarados. Próxima cita en 6 meses.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[11].Id, MedicoId = medicos[5].Id, CitaId = completedCitas[13].Id, Fecha = completedCitas[13].FechaHora,
                Diagnostico = "<p>Evaluación post-blanqueamiento. Tono dental actual A1 en escala Vita (era A3 inicial). Resultado satisfactorio. Ligeras manchas blancas por fluorosis leve en incisivos centrales.</p>",
                Tratamiento = "<p>Aplicación de gel remineralizante con fosfato de calcio amorfo (MI Paste Plus). Microabrasión con ácido clorhídrico y piedra pómez en manchas de fluorosis.</p>",
                Observaciones = "<p>Resultado estético muy bueno. Se recomienda mantenimiento con férula de blanqueamiento 1 vez al mes. Evitar café, té y vino tinto por 48 horas. Próxima cita en 6 meses.</p>"
            });
            historiales.Add(new HistorialMedico
            {
                PacienteId = pacientes[12].Id, MedicoId = medicos[0].Id, CitaId = completedCitas[14].Id, Fecha = completedCitas[14].FechaHora,
                Diagnostico = "<p>Maloclusión clase I con apiñamiento severo superior e inferior. Mordida profunda. Línea media coincidente.</p>",
                Tratamiento = "<p><strong>Cementación de brackets metálicos Roth 0.022\":</strong></p><ul><li>Colocación de separadores elásticos en molares</li><li>Acondicionamiento ácido y bonding</li><li>Cementación de brackets superiores e inferiores</li><li>Colocación de arco inicial NiTi 0.014\"</li><li>Ligaduras elásticas</li></ul>",
                Observaciones = "<p>Paciente toleró bien el procedimiento. Se explicó higiene con brackets y dieta. Próxima cita en 6 semanas para cambio de arco. Dolor leve esperado por 3-5 días.</p>"
            });

            context.HistorialMedicos.AddRange(historiales);
            await context.SaveChangesAsync();
        }
    }
}

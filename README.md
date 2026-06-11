# MuelitasFelices

Sistema de gestion de citas odontologicas desarrollado con ASP.NET Core MVC 8, Entity Framework Core y SQL Server LocalDB.

## Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 8.0)
- **ORM:** Entity Framework Core 8.0.11 (Code-First)
- **Autenticación:** ASP.NET Identity con roles (Admin, Medico, Paciente)
- **Base de datos:** SQL Server LocalDB
- **UI:** Bootstrap 5 + Bootstrap Icons
- **Editor de texto:** TinyMCE (historial medico)

## Funcionalidades

### Roles

- **Admin:** Gestion de usuarios, medicos, especialidades y citas. Dashboard con KPIs.
- **Medico:** Dashboard de citas propias, pacientes asignados, historial medico con TinyMCE.
- **Paciente:** Agendar/reprogramar/cancelar citas, historial medico personal.

### Modelos principales

- `Usuario` (extiende IdentityUser) — usuarios del sistema
- `Paciente` — datos adicionales del paciente
- `Medico` — datos del medico con licencia y especialidades (relacion N:M)
- `Especialidad` — catalogos (Odontologia General, Ortodoncia, Endodoncia, etc.)
- `Cita` — con tipos (General, PorHistorial, Remitido) y estados (Pendiente, Confirmada, Completada, Cancelada)
- `HistorialMedico` — registro de diagnosticos y tratamientos

### Seed data

La base se inicializa automaticamente al iniciar la aplicacion con:
- 1 administrador
- 6 medicos (con especialidades)
- 20 pacientes
- 28 citas (15 completadas, 6 pendientes, 4 canceladas, 3 confirmadas)
- 15 entradas de historial medico

## Como ejecutar

1. Clonar el repositorio
2. Abrir la solucion en Visual Studio 2022 o terminal
3. Restaurar paquetes:
   ```powershell
   dotnet restore
   ```
4. Aplicar migraciones (la BD se crea automaticamente):
   ```powershell
   dotnet ef database update
   ```
5. Ejecutar:
   ```powershell
   dotnet run
   ```
6. Navegar a `https://localhost:5001`

## Usuarios de prueba

### Administrador
| Email | Contrasena |
|-------|-----------|
| admin@muelitas.com | Admin123! |

### Medicos
| Nombre | Email | Contrasena |
|--------|-------|-----------|
| Dr. Carlos Lopez | carlos.lopez@muelitas.com | Medico123! |
| Dra. Maria Garcia | maria.garcia@muelitas.com | Medico123! |
| Dr. Juan Perez | juan.perez@muelitas.com | Medico123! |
| Dra. Ana Martinez | ana.martinez@muelitas.com | Medico123! |
| Dr. Roberto Sanchez | roberto.sanchez@muelitas.com | Medico123! |
| Dra. Laura Torres | laura.torres@muelitas.com | Medico123! |

### Pacientes (contrasena: Paciente123!)
pedro.rodriguez@email.com, maria.fernandez@email.com, jose.castillo@email.com, ana.reyes@email.com, luis.martinez@email.com, carmen.diaz@email.com, carlos.hernandez@email.com, laura.sanchez@email.com, miguel.torres@email.com, sofia.ramirez@email.com, diego.morales@email.com, valentina.ortiz@email.com, andres.vega@email.com, gabriela.nunez@email.com, fernando.pena@email.com, isabel.guzman@email.com, ricardo.flores@email.com, patricia.medina@email.com, jorge.rivas@email.com, daniela.campos@email.com

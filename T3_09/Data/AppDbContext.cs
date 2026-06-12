using Microsoft.EntityFrameworkCore;
using T3_09.Models;

namespace T3_09.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Rol> Rols { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }  
        public DbSet<Vacante> Vacantes { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }
        public DbSet<Pago> Pagos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Matricula>()
            .HasOne(m => m.Usuario)
            .WithMany()
            .HasForeignKey(m => m.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>()
                .HasOne(r => r.Rol)
                .WithMany(y => y.Usuarios)
                .HasForeignKey(r => r.idRol);

            // Roles predefinidos para la creación de usuarios
            modelBuilder.Entity<Rol>().HasData(
                new Rol { IdRol = 1, NomRol = "Administrador", DesRol = "Administrador" },
                new Rol { IdRol = 2, NomRol = "Usuario", DesRol = "Padre de Familia" }
            );

            //Usuario Administrador
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    IdUsuario = 1,
                    NomUsuario = "Adminsanandres",
                    ApeUsuario = "Sistema", 
                    Correo = "admin@sanandres.edu.pe", 
                    Password = "admin720650",
                    idRol = 1 
                }
            );         
            modelBuilder.Entity<Vacante>().HasData(
                new Vacante { IdVacante = 1, Grado = "1ro Secundaria", Seccion = "A", CuposDisponibles = 28 },
                new Vacante { IdVacante = 2, Grado = "1ro Secundaria", Seccion = "B", CuposDisponibles = 15 },
                new Vacante { IdVacante = 3, Grado = "1ro Secundaria", Seccion = "C", CuposDisponibles = 5 },

                new Vacante { IdVacante = 4, Grado = "2do Secundaria", Seccion = "A", CuposDisponibles = 30 },
                new Vacante { IdVacante = 5, Grado = "2do Secundaria", Seccion = "B", CuposDisponibles = 22 },
                new Vacante { IdVacante = 6, Grado = "2do Secundaria", Seccion = "C", CuposDisponibles = 10 },

                new Vacante { IdVacante = 7, Grado = "3ro Secundaria", Seccion = "A", CuposDisponibles = 18 },
                new Vacante { IdVacante = 8, Grado = "3ro Secundaria", Seccion = "B", CuposDisponibles = 0 }, 
                new Vacante { IdVacante = 9, Grado = "3ro Secundaria", Seccion = "C", CuposDisponibles = 25 },

                new Vacante { IdVacante = 10, Grado = "4to Secundaria", Seccion = "A", CuposDisponibles = 12 },
                new Vacante { IdVacante = 11, Grado = "4to Secundaria", Seccion = "B", CuposDisponibles = 29 },
                new Vacante { IdVacante = 12, Grado = "4to Secundaria", Seccion = "C", CuposDisponibles = 8 },

                new Vacante { IdVacante = 13, Grado = "5to Secundaria", Seccion = "A", CuposDisponibles = 20 },
                new Vacante { IdVacante = 14, Grado = "5to Secundaria", Seccion = "B", CuposDisponibles = 14 },
                new Vacante { IdVacante = 15, Grado = "5to Secundaria", Seccion = "C", CuposDisponibles = 2 }
            );
        }
    }
}

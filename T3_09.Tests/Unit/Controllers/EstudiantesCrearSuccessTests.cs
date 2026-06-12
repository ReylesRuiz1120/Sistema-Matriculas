using System.Linq;
using Microsoft.AspNetCore.Mvc;
using T3_09.Controllers;
using T3_09.Models;
using T3_09.Tests.Helpers;
using Xunit;

namespace T3_09.Tests.Unit.Controllers
{
    // Test successful creation of estudiante when no vacante is selected
    public class EstudiantesCrearSuccessTests
    {
        [Fact]
        public async void Crear_Adds_Student_When_Valid_NoVacante()
        {
            var ctx = TestDbContextFactory.Create("est_crear_success");
            var controller = new EstudiantesController(ctx);
            var estudiante = new Estudiante { NombreCompleto = "Carlos Lopez", Dni = "12121212", FechaNacimiento = System.DateTime.Now.AddYears(-10), NombreApoderado = "Padre" };

            var result = await controller.Crear(estudiante, null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Listar", redirect.ActionName);
            var created = ctx.Estudiantes.FirstOrDefault(e => e.Dni == "12121212");
            Assert.NotNull(created);
        }
    }
}

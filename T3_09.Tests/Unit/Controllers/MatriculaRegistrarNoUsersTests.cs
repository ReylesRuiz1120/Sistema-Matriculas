using Microsoft.AspNetCore.Mvc;
using T3_09.Controllers;
using T3_09.ViewModels;
using T3_09.Tests.Helpers;
using Xunit;

namespace T3_09.Tests.Unit.Controllers
{
    // When there are no users in DB, Registrar should redirect to Login (Acceso)
    public class MatriculaRegistrarNoUsersTests
    {
        [Fact]
        public async void Registrar_Redirects_To_Login_When_No_User()
        {
            var ctx = TestDbContextFactory.Create("mat_registrar_nousers");
            // remove seeded users
            foreach (var u in ctx.Usuarios.ToList()) ctx.Usuarios.Remove(u);
            ctx.SaveChanges();

            var controller = new MatriculaController(ctx);
            var model = new ProcesoMatriculaVM { IdVacanteSeleccionada = 1, Dni = "13131313", NombreCompleto = "SinUser", FechaNacimiento = System.DateTime.Now.AddYears(-10), NombreApoderado = "P" };
            var result = await controller.Registrar(model);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Acceso", redirect.ControllerName);
        }
    }
}

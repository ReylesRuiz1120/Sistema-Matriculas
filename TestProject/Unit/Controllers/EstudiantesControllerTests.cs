using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProject.Helpers;
using T3_09.Controllers;
using T3_09.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace TestProject.Unit.Controllers
{
    /*
     * Pruebas para `EstudiantesController` que cubren:
     * - Filtrado de lista por DNI.
     * - Visualización de detalles y manejo de nulos.
     * - Validaciones en creación (edad, Dni duplicado) y flujo de alta sin vacante.
     * - Edición y eliminación de estudiantes.
     * Objetivo: asegurar la correcta gestión de estudiantes y reglas de negocio asociadas.
     */
    [TestClass]
    public class EstudiantesControllerTests
    {
        [TestMethod]
        // Valida que `Listar` filtra correctamente por fragmento de DNI.
        public async System.Threading.Tasks.Task Listar_Filters_By_Dni()
        {
            var ctx = TestDbContextFactory.Create("est_listar");
            ctx.Estudiantes.Add(new Estudiante { NombreCompleto = "A", Dni = "11111111", FechaNacimiento = System.DateTime.Today.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" });
            ctx.Estudiantes.Add(new Estudiante { NombreCompleto = "B", Dni = "22222222", FechaNacimiento = System.DateTime.Today.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" });
            ctx.SaveChanges();

            var controller = new EstudiantesController(ctx);
            var result = await controller.Listar("1111") as ViewResult;
            Assert.IsNotNull(result);
            var model = result.Model as System.Collections.IEnumerable;
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Cast<object>().Count());
        }

        [TestMethod]
        // Si `id` es null en `Detalles`, debe devolver NotFound.
        public async System.Threading.Tasks.Task Detalles_Returns_NotFound_ForNull()
        {
            var ctx = TestDbContextFactory.Create("est_det_null");
            var controller = new EstudiantesController(ctx);
            var result = await controller.Detalles(null);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        // Al crear, si la edad está fuera del rango permitido, ModelState debe ser inválido.
        public async System.Threading.Tasks.Task Crear_Adds_Error_When_Age_Out_Of_Range()
        {
            var ctx = TestDbContextFactory.Create("est_crear_age");
            var controller = new EstudiantesController(ctx);
            var estudiante = new Estudiante { NombreCompleto = "X", Dni = "33333333", FechaNacimiento = System.DateTime.Now.AddYears(-2), Direccion = "Calle", NombreApoderado = "P" };
            var result = await controller.Crear(estudiante, null) as ViewResult;
            Assert.IsNotNull(result);
            Assert.IsFalse(controller.ModelState.IsValid);
        }

        [TestMethod]
        // Al crear, si el DNI ya existe, debe añadirse un error de ModelState.
        public async System.Threading.Tasks.Task Crear_Adds_Error_When_Dni_Exists()
        {
            var ctx = TestDbContextFactory.Create("est_crear_dni");
            ctx.Estudiantes.Add(new Estudiante { NombreCompleto = "A", Dni = "44444444", FechaNacimiento = System.DateTime.Today.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" });
            ctx.SaveChanges();
            var controller = new EstudiantesController(ctx);
            var estudiante = new Estudiante { NombreCompleto = "X", Dni = "44444444", FechaNacimiento = System.DateTime.Now.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" };
            var result = await controller.Crear(estudiante, null) as ViewResult;
            Assert.IsNotNull(result);
            Assert.IsFalse(controller.ModelState.IsValid);
        }

        [TestMethod]
        // Flujo exitoso de creación sin seleccionar vacante: se debe redirigir a Listar y persistir el estudiante.
        public async System.Threading.Tasks.Task Crear_Success_NoVacante_AddsStudent()
        {
            var ctx = TestDbContextFactory.Create("est_crear_success");
            var controller = new EstudiantesController(ctx);
            var estudiante = new Estudiante { NombreCompleto = "Carlos Lopez", Dni = "12121212", FechaNacimiento = System.DateTime.Now.AddYears(-10), Direccion = "Calle", NombreApoderado = "Padre" };
            var result = await controller.Crear(estudiante, null) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Listar", result.ActionName);
            var created = ctx.Estudiantes.FirstOrDefault(e => e.Dni == "12121212");
            Assert.IsNotNull(created);
        }

        [TestMethod]
        // Editar (POST) con id distinto al modelo debe devolver NotFound.
        public async System.Threading.Tasks.Task Editar_Post_Id_Mismatch_Returns_NotFound()
        {
            var ctx = TestDbContextFactory.Create("est_edit_mismatch");
            var controller = new EstudiantesController(ctx);
            var estudiante = new Estudiante { IdEstudiante = 5, NombreCompleto = "X", Dni = "55555555", FechaNacimiento = System.DateTime.Now.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" };
            var result = await controller.Editar(1, estudiante);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        // Confirmar eliminación elimina el estudiante y redirige a Listar.
        public async System.Threading.Tasks.Task ConfirmarEliminacion_Removes_Student()
        {
            var ctx = TestDbContextFactory.Create("est_eliminar");
            var s = new Estudiante { NombreCompleto = "ToRemove", Dni = "66666666", FechaNacimiento = System.DateTime.Today.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" };
            ctx.Estudiantes.Add(s);
            ctx.SaveChanges();
            var controller = new EstudiantesController(ctx);
            var result = await controller.ConfirmarEliminacion(s.IdEstudiante) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Listar", result.ActionName);
            Assert.AreEqual(0, ctx.Estudiantes.Count());
        }

        [TestMethod]
        // GET Editar debe devolver la vista con el estudiante cuando existe.
        public async System.Threading.Tasks.Task Editar_Get_Returns_View_When_Found()
        {
            var ctx = TestDbContextFactory.Create("est_edit_get");
            var s = new Estudiante { NombreCompleto = "E", Dni = "77777777", FechaNacimiento = System.DateTime.Today.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" };
            ctx.Estudiantes.Add(s);
            ctx.SaveChanges();
            var controller = new EstudiantesController(ctx);
            var result = await controller.Editar(s.IdEstudiante) as ViewResult;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(Estudiante));
        }

        [TestMethod]
        // GET Eliminar con id null debe devolver NotFound.
        public async System.Threading.Tasks.Task Eliminar_Get_Returns_NotFound_For_Null()
        {
            var ctx = TestDbContextFactory.Create("est_del_null");
            var controller = new EstudiantesController(ctx);
            var result = await controller.Eliminar(null);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProject.Helpers;
using T3_09.Controllers;
using T3_09.ViewModels;
using T3_09.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TestProject.Unit.Controllers
{
    /*
     * Pruebas para `MatriculaController` que cubren:
     * - Selección y carga de formulario de matrícula.
     * - Registro de matrícula en diferentes escenarios (modelo inválido, alumno ya existente, éxito, sin usuarios).
     * - Aceptación de matrícula y comprobante.
     * Objetivo: validar el proceso de matrícula y efectos sobre `Vacante` y `Matricula`.
     */
    [TestClass]
    public class MatriculaControllerTests
    {
        [TestMethod]
        // Verifica que la vista de selección contiene la lista de vacantes disponibles.
        public void Seleccion_Returns_Vacantes_List()
        {
            var ctx = TestDbContextFactory.Create("mat_seleccion");
            var controller = new MatriculaController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var result = controller.Seleccion() as ViewResult;
            Assert.IsNotNull(result);
            Assert.IsNotNull(controller.ViewBag.ListaVacantes);
        }

        [TestMethod]
        // Si se solicita cargar formulario para una vacante inválida, debe redirigir a Seleccion.
        public void CargarFormulario_InvalidVacante_Redirects()
        {
            var ctx = TestDbContextFactory.Create("mat_cargar_invalid");
            var controller = new MatriculaController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var result = controller.CargarFormulario(999) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Seleccion", result.ActionName);
        }

        [TestMethod]
        // CargarFormulario debe preparar un modelo con InfoVacante basada en la vacante seleccionada.
        public void CargarFormulario_Sets_Model_InfoVacante()
        {
            var ctx = TestDbContextFactory.Create("mat_cargar_set");
            var vac = ctx.Vacantes.First(v => v.CuposDisponibles > 0);
            var controller = new MatriculaController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var result = controller.CargarFormulario(vac.IdVacante) as ViewResult;
            Assert.IsNotNull(result);
            var model = result.Model as ProcesoMatriculaVM;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.InfoVacante.Contains(vac.Grado));
        }

        [TestMethod]
        // Si el modelo es inválido, Registrar debe devolver la vista del formulario con el mismo modelo.
        public async System.Threading.Tasks.Task Registrar_Returns_Form_When_ModelInvalid()
        {
            var ctx = TestDbContextFactory.Create("mat_reg_invalid");
            var controller = new MatriculaController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            controller.ModelState.AddModelError("Dni", "Required");
            var modelo = new ProcesoMatriculaVM { IdVacanteSeleccionada = 1 };
            var result = await controller.Registrar(modelo) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Formulario", result.ViewName);
        }

        [TestMethod]
        // Si el estudiante ya tiene una matrícula pendiente o matriculado, Registrar debe devolver la vista con un error.
        public async System.Threading.Tasks.Task Registrar_Returns_View_When_StudentAlreadyExists()
        {
            var ctx = TestDbContextFactory.Create("mat_reg_exists");
            var est = new Estudiante { NombreCompleto = "E", Dni = "77777777", FechaNacimiento = System.DateTime.Today.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" };
            ctx.Estudiantes.Add(est);
            ctx.Matriculas.Add(new Matricula { Estudiante = est, Estado = "Pendiente", FechaRegistro = System.DateTime.Now, CodigoPago = "X", IdUsuario = 1, IdVacante = 1 });
            ctx.SaveChanges();
            var controller = new MatriculaController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var modelo = new ProcesoMatriculaVM { IdVacanteSeleccionada = 1, Dni = "77777777", NombreCompleto = "E", FechaNacimiento = System.DateTime.Now.AddYears(-10), NombreApoderado = "P" };
            var result = await controller.Registrar(modelo) as ViewResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(controller.ViewData.ContainsKey("Error"));
        }

        [TestMethod]
        // Caso de éxito: se crea la matrícula y se decrementa el cupo de la vacante.
        public async System.Threading.Tasks.Task Registrar_Success_Creates_Matricula_And_Decrements_Vacante()
        {
            var ctx = TestDbContextFactory.Create("mat_reg_success");
            var vac = ctx.Vacantes.First(v => v.CuposDisponibles > 0);
            int before = vac.CuposDisponibles;
            var controller = new MatriculaController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "admin@sanandres.edu.pe") }, "Test"));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
            var modelo = new ProcesoMatriculaVM { IdVacanteSeleccionada = vac.IdVacante, Dni = "88888888", NombreCompleto = "Nuevo", FechaNacimiento = System.DateTime.Now.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" };
            var result = await controller.Registrar(modelo) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Comprobante", result.ActionName);
            var updatedVac = ctx.Vacantes.Find(vac.IdVacante);
            Assert.AreEqual(before - 1, updatedVac.CuposDisponibles);
        }

        [TestMethod]
        // Si no hay usuarios en la BD, Registrar debe redirigir al Login (Acceso).
        public async System.Threading.Tasks.Task Registrar_Redirects_To_Login_When_No_User()
        {
            var ctx = TestDbContextFactory.Create("mat_reg_nousers");
            foreach (var u in ctx.Usuarios.ToList()) ctx.Usuarios.Remove(u);
            ctx.SaveChanges();
            var controller = new MatriculaController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var model = new ProcesoMatriculaVM { IdVacanteSeleccionada = 1, Dni = "13131313", NombreCompleto = "SinUser", FechaNacimiento = System.DateTime.Now.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" };
            var result = await controller.Registrar(model) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Login", result.ActionName);
            Assert.AreEqual("Acceso", result.ControllerName);
        }

        [TestMethod]
        // Al aceptar una matrícula pendiente, el estado debe pasar a "Matriculado" y debe añadirse un Pago.
        public async System.Threading.Tasks.Task AceptarMatricula_SetsEstado_And_AddsPago()
        {
            var ctx = TestDbContextFactory.Create("mat_aceptar");
            var est = new Estudiante { NombreCompleto = "E2", Dni = "99999999", FechaNacimiento = System.DateTime.Today.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" };
            ctx.Estudiantes.Add(est);
            var vac = ctx.Vacantes.First(v => v.CuposDisponibles > 0);
            var m = new Matricula { Estudiante = est, Estado = "Pendiente", FechaRegistro = System.DateTime.Now, CodigoPago = "C1", IdUsuario = 1, IdVacante = vac.IdVacante };
            ctx.Matriculas.Add(m);
            ctx.SaveChanges();
            var controller = new MatriculaController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var result = await controller.AceptarMatricula(m.IdMatricula) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("SolicitudesPendientes", result.ActionName);
            var updated = ctx.Matriculas.First(x => x.IdMatricula == m.IdMatricula);
            Assert.AreEqual("Matriculado", updated.Estado);
            Assert.IsNotNull(updated.Pago);
        }

        [TestMethod]
        // Si no existe la matrícula, Comprobante debe redirigir a Seleccion.
        public void Comprobante_Redirects_When_Not_Found()
        {
            var ctx = TestDbContextFactory.Create("mat_comp_notfound");
            var controller = new MatriculaController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var result = controller.Comprobante(999) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Seleccion", result.ActionName);
        }

        [TestMethod]
        // Si la matrícula existe, Comprobante debe devolver la vista con el modelo.
        public void Comprobante_Returns_View_When_Found()
        {
            var ctx = TestDbContextFactory.Create("mat_comp_found");
            var est = new Estudiante { NombreCompleto = "E3", Dni = "10101010", FechaNacimiento = System.DateTime.Today.AddYears(-10), Direccion = "Calle", NombreApoderado = "P" };
            ctx.Estudiantes.Add(est);
            var vac = ctx.Vacantes.First(v => v.CuposDisponibles > 0);
            var m = new Matricula { Estudiante = est, Estado = "Pendiente", FechaRegistro = System.DateTime.Now, CodigoPago = "C2", IdUsuario = 1, IdVacante = vac.IdVacante };
            ctx.Matriculas.Add(m);
            ctx.SaveChanges();
            var controller = new MatriculaController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var result = controller.Comprobante(m.IdMatricula) as ViewResult;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Model);
        }

        [TestMethod]
        // SolicitudesPendientes debe devolver la lista de matrículas en estado Pendiente.
        public async System.Threading.Tasks.Task SolicitudesPendientes_Returns_List()
        {
            var ctx = TestDbContextFactory.Create("mat_solicitudes");
            var vac = ctx.Vacantes.First(v => v.CuposDisponibles > 0);
            ctx.Matriculas.Add(new Matricula { Estado = "Pendiente", FechaRegistro = System.DateTime.Now, CodigoPago = "P1", IdUsuario = 1, IdVacante = vac.IdVacante, Estudiante = new Estudiante { NombreCompleto = "P", Dni = "14141414", FechaNacimiento = System.DateTime.Today.AddYears(-10), Direccion = "Calle", NombreApoderado = "X" } });
            ctx.SaveChanges();
            var controller = new MatriculaController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var result = await controller.SolicitudesPendientes() as ViewResult;
            Assert.IsNotNull(result);
            var model = result.Model as System.Collections.IEnumerable;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Cast<object>().Any());
        }
    }
}

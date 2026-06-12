using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3_09.Controllers;
using T3_09.ViewModels;
using TestProject.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TestProject.Unit.Controllers
{
    /*
     * Pruebas para `AccesoController` que cubren:
     * - Flujo de registro (casos de email existente, contraseñas no coinciden, ModelState inválido y éxito).
     * - Flujo de login con redirecciones según el rol del usuario.
     * Objetivo: validar la lógica de control de acceso y mensajes de error.
     */
    [TestClass]
    public class AccesoControllerTests
    {
        [TestMethod]
        // Si el correo ya existe, se debe devolver la vista con el mismo modelo y mostrar mensaje.
        public async System.Threading.Tasks.Task Registro_ReturnsView_When_EmailExists()
        {
            var ctx = TestDbContextFactory.Create("acc_reg_email");
            var controller = new AccesoController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var vm = new UsuarioVM { Nombre = "X", Apellido = "Y", Correo = "admin@sanandres.edu.pe", Contraseña = "p", Repite_Contraseña = "p", Id_Rol = 2 };
            var result = await controller.Registro(vm);
            var view = result as ViewResult;
            Assert.IsNotNull(view);
            Assert.AreEqual(vm, view.Model);
        }

        [TestMethod]
        // Si las contraseñas no coinciden, debe devolver la vista y establecer ViewData["Mensaje"].
        public async System.Threading.Tasks.Task Registro_ReturnsView_When_PasswordsDontMatch()
        {
            var ctx = TestDbContextFactory.Create("acc_reg_pass");
            var controller = new AccesoController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var vm = new UsuarioVM { Nombre = "X", Apellido = "Y", Correo = "new@ex.com", Contraseña = "a", Repite_Contraseña = "b", Id_Rol = 2 };
            var result = await controller.Registro(vm);
            var view = result as ViewResult;
            Assert.IsNotNull(view);
            Assert.IsTrue(controller.ViewData.ContainsKey("Mensaje"));
        }

        [TestMethod]
        // Si ModelState es inválido, la acción de registro debe devolver la vista sin procesar.
        public async System.Threading.Tasks.Task Registro_ModelStateInvalid_ReturnsView()
        {
            var ctx = TestDbContextFactory.Create("acc_reg_invalid");
            var controller = new AccesoController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            controller.ModelState.AddModelError("Correo", "Required");
            var vm = new UsuarioVM();
            var result = await controller.Registro(vm);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        // Credenciales inválidas en login deben devolver la vista y mostrar mensaje.
        public async System.Threading.Tasks.Task Login_Post_InvalidCredentials_ReturnsViewWithMessage()
        {
            var ctx = TestDbContextFactory.Create("acc_login_invalid");
            var controller = new AccesoController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var vm = new LoginVM { Correo = "nope@x.com", Contraseña = "wrong" };
            var result = await controller.Login(vm);
            var view = result as ViewResult;
            Assert.IsNotNull(view);
            Assert.IsTrue(controller.ViewData.ContainsKey("Mensaje"));
        }

        [TestMethod]
        // Login con credenciales del admin debe redirigir a la lista de estudiantes (rol Administrador).
        public async System.Threading.Tasks.Task Login_Post_Admin_Redirects()
        {
            var ctx = TestDbContextFactory.Create("acc_login_admin");
            var controller = new AccesoController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var vm = new LoginVM { Correo = "admin@sanandres.edu.pe", Contraseña = "admin720650" };
            var result = await controller.Login(vm) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Listar", result.ActionName);
            Assert.AreEqual("Estudiantes", result.ControllerName);
        }

        [TestMethod]
        // Login para rol Usuario debe redirigir a la selección de matrícula.
        public async System.Threading.Tasks.Task Login_Post_User_Redirects_To_Matricula()
        {
            var ctx = TestDbContextFactory.Create("acc_login_user");
            // No crear rol 2: ya existe en los datos semilla.
            ctx.Usuarios.Add(new T3_09.Models.Usuario { NomUsuario = "U", ApeUsuario = "A", Correo = "u@x.com", Password = "p", idRol = 2 });
            ctx.SaveChanges();
            var controller = new AccesoController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var vm = new LoginVM { Correo = "u@x.com", Contraseña = "p" };
            var result = await controller.Login(vm) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Seleccion", result.ActionName);
            Assert.AreEqual("Matricula", result.ControllerName);
        }

        [TestMethod]
        // Login para rol Visitante debe redirigir al controlador Visit.
        public async System.Threading.Tasks.Task Login_Post_Visitante_Redirects_To_Visit()
        {
            var ctx = TestDbContextFactory.Create("acc_login_visit");
            ctx.Rols.Add(new T3_09.Models.Rol { IdRol = 3, NomRol = "Visitante", DesRol = "Visit" });
            ctx.Usuarios.Add(new T3_09.Models.Usuario { NomUsuario = "V", ApeUsuario = "A", Correo = "v@x.com", Password = "p", idRol = 3 });
            ctx.SaveChanges();
            var controller = new AccesoController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var vm = new LoginVM { Correo = "v@x.com", Contraseña = "p" };
            var result = await controller.Login(vm) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Visit", result.ControllerName);
        }

        [TestMethod]
        // Registro exitoso debe crear un usuario en la BD y redirigir al Login.
        public async System.Threading.Tasks.Task Registro_Success_Creates_User_And_Redirects_To_Login()
        {
            var ctx = TestDbContextFactory.Create("acc_reg_success");
            var controller = new AccesoController(ctx);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider());
            var vm = new UsuarioVM { Nombre = "New", Apellido = "User", Correo = "new@x.com", Contraseña = "p", Repite_Contraseña = "p", Id_Rol = 2 };
            var result = await controller.Registro(vm) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Login", result.ActionName);
            var created = ctx.Usuarios.FirstOrDefault(u => u.Correo == "new@x.com");
            Assert.IsNotNull(created);
        }
    }
}

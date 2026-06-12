using Microsoft.VisualStudio.TestTools.UnitTesting;
using T3_09.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TestProject.Helpers;
using System.Linq;

namespace TestProject.Unit.Models
{
    /*
     * Conjunto de pruebas unitarias para modelos simples del dominio:
     * - Verifican comportamiento de métodos (como `ReservarCupo`/`LiberarCupo`).
     * - Validaciones aplicadas por DataAnnotations en modelos como `Estudiante` y `Usuario`.
     * Objetivo: detectar rupturas en reglas de negocio y validaciones de entrada.
     */
    [TestClass]
    public class ModelTests
    {
        private IList<ValidationResult> Validate(object model)
        {
            var ctx = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, ctx, results, true);
            return results;
        }

        [TestMethod]
        // Verifica que `ReservarCupo` decrementa `CuposDisponibles` en 1.
        public void Vacante_Reservar_Decrements_Cupos()
        {
            var v = new Vacante { CuposDisponibles = 3 };
            v.ReservarCupo();
            Assert.AreEqual(2, v.CuposDisponibles);
        }

        [TestMethod]
        // Verifica que `LiberarCupo` incrementa `CuposDisponibles` en 1.
        public void Vacante_Liberar_Increments_Cupos()
        {
            var v = new Vacante { CuposDisponibles = 1 };
            v.LiberarCupo();
            Assert.AreEqual(2, v.CuposDisponibles);
        }

        [TestMethod]
        // Documenta el comportamiento actual: reservar cuando no hay cupos permite valores negativos.
        public void Vacante_Allows_Negative_When_OverReserved()
        {
            var v = new Vacante { CuposDisponibles = 0 };
            v.ReservarCupo();
            Assert.AreEqual(-1, v.CuposDisponibles);
        }

        [TestMethod]
        // Valida que un `Estudiante` con datos correctos satisface las DataAnnotations.
        public void Estudiante_Valid_Passes_Validation()
        {
            var e = new Estudiante
            {
                NombreCompleto = "Juan Perez",
                Dni = "12345678",
                FechaNacimiento = System.DateTime.Today.AddYears(-10),
                Direccion = "Calle 1",
                NombreApoderado = "Apoderado"
            };
            var results = Validate(e);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        // Verifica que un DNI inválido (no numérico o longitud incorrecta) falla la validación.
        public void Estudiante_Invalid_Dni_Fails()
        {
            var e = new Estudiante { NombreCompleto = "Ana", Dni = "ABC", FechaNacimiento = System.DateTime.Today, Direccion = "Calle", NombreApoderado = "P" };
            var results = Validate(e);
            Assert.IsTrue(results.Any(r => r.ErrorMessage != null && r.ErrorMessage.Contains("DNI")));
        }

        [TestMethod]
        // Verifica restricción de longitud del campo `NombreCompleto`.
        public void Estudiante_LongName_Fails_StringLength()
        {
            var e = new Estudiante { NombreCompleto = new string('A', 201), Dni = "12345678", FechaNacimiento = System.DateTime.Today, Direccion = "Calle", NombreApoderado = "P" };
            var results = Validate(e);
            Assert.IsTrue(results.Count > 0);
        }

        [TestMethod]
        // `Usuario` sin campos requeridos debe fallar las validaciones.
        public void Usuario_Required_Fields_Fail()
        {
            var u = new Usuario();
            var results = Validate(u);
            Assert.IsTrue(results.Count > 0);
        }

        [TestMethod]
        // Verifica que una contraseña más larga que el límite de la anotación falla.
        public void Usuario_LongPassword_Fails_StringLength()
        {
            var u = new Usuario { NomUsuario = "N", ApeUsuario = "A", Correo = "a@b.com", Password = new string('x', 30), idRol = 1 };
            var results = Validate(u);
            Assert.IsTrue(results.Count > 0);
        }

        [TestMethod]
        // Usuario con campos válidos debe pasar validación.
        public void Usuario_Valid_Passes()
        {
            var u = new Usuario { NomUsuario = "N", ApeUsuario = "A", Correo = "a@b.com", Password = "p", idRol = 1 };
            var results = Validate(u);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        // Comprueba que propiedades simples en Pago se asignan correctamente.
        public void Pago_Properties_Are_Set()
        {
            var p = new Pago { CodigoPago = "X", FechaPago = System.DateTime.Today, EntidadBancaria = "Bank", Monto = 99.9 };
            Assert.AreEqual("X", p.CodigoPago);
            Assert.AreEqual("Bank", p.EntidadBancaria);
            Assert.AreEqual(99.9, p.Monto);
        }

        [TestMethod]
        // Verifica creación de Rol y que la colección `Usuarios` viene nula por defecto.
        public void Rol_Creation_And_Default_Usuarios_Null()
        {
            var r = new Rol { NomRol = "Profesor", DesRol = "Docente" };
            Assert.AreEqual("Profesor", r.NomRol);
            Assert.IsNull(r.Usuarios);
        }
    }
}

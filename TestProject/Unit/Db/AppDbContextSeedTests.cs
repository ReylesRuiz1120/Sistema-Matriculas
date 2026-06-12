using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProject.Helpers;
using System.Linq;

namespace TestProject.Unit.Db
{
    /*
     * Pruebas que verifican los datos semilla cargados en `AppDbContext`.
     * Objetivo: asegurar que las entidades necesarias (roles, usuario admin, vacantes) existan tras crear el contexto.
     */
    [TestClass]
    public class AppDbContextSeedTests
    {
        [TestMethod]
        // Comprueba que existen roles predefinidos (Administrador y Usuario).
        public void Seed_Creates_Predefined_Roles()
        {
            var ctx = TestDbContextFactory.Create("seed_roles");
            var roles = ctx.Rols.ToList();
            Assert.IsTrue(roles.Count >= 2);
            Assert.IsTrue(roles.Any(r => r.NomRol == "Administrador"));
        }

        [TestMethod]
        // Comprueba que el usuario administrador semilla está presente con idRol = 1.
        public void Seed_Creates_Admin_User()
        {
            var ctx = TestDbContextFactory.Create("seed_admin");
            var admin = ctx.Usuarios.FirstOrDefault(u => u.Correo == "admin@sanandres.edu.pe");
            Assert.IsNotNull(admin);
            Assert.AreEqual(1, admin.idRol);
        }

        [TestMethod]
        // Verifica que la vacante con IdVacante = 1 existe en el seed.
        public void Seed_Includes_Vacante_Id1()
        {
            var ctx = TestDbContextFactory.Create("seed_vac1");
            var vac = ctx.Vacantes.FirstOrDefault(v => v.IdVacante == 1);
            Assert.IsNotNull(vac);
        }
    }
}

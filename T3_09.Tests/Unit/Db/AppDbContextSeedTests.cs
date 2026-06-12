using System.Linq;
using Xunit;
using T3_09.Tests.Helpers;

namespace T3_09.Tests.Unit.Db
{
    // Verify that AppDbContext seed data contains expected roles and admin user
    public class AppDbContextSeedTests
    {
        [Fact]
        public void Seed_Creates_Predefined_Roles()
        {
            var ctx = TestDbContextFactory.Create("seed_roles");
            var roles = ctx.Rols.ToList();
            Assert.True(roles.Count >= 2);
            Assert.Contains(roles, r => r.NomRol == "Administrador");
        }

        [Fact]
        public void Seed_Creates_Admin_User()
        {
            var ctx = TestDbContextFactory.Create("seed_admin");
            var admin = ctx.Usuarios.FirstOrDefault(u => u.Correo == "admin@sanandres.edu.pe");
            Assert.NotNull(admin);
            Assert.Equal(1, admin.idRol);
        }
    }
}

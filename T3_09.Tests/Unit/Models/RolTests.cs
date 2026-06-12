using Xunit;
using T3_09.Models;

namespace T3_09.Tests.Unit.Models
{
    // Tests for Rol model basic behavior
    public class RolTests
    {
        [Fact]
        public void Can_Create_Rol_With_Properties()
        {
            var r = new Rol { NomRol = "Profesor", DesRol = "Docente" };
            Assert.Equal("Profesor", r.NomRol);
            Assert.Equal("Docente", r.DesRol);
        }

        [Fact]
        public void Usuarios_Collection_Is_Null_By_Default()
        {
            var r = new Rol();
            Assert.Null(r.Usuarios);
        }
    }
}

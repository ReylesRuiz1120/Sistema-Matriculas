using Microsoft.EntityFrameworkCore;
using T3_09.Data;

namespace TestProject.Helpers
{
    /// <summary>
    /// Helper para crear instancias de `AppDbContext` usando el proveedor InMemory.
    /// Objetivo: ejecutar pruebas aisladas sin depender de una base de datos real.
    /// Cada prueba debe usar una base con nombre único para evitar interferencias.
    /// </summary>
    public static class TestDbContextFactory
    {
        public static AppDbContext Create(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: name)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;
        }
    }
}

using Ferreteria.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ferreteria.Tests.Comun
{
    public abstract class BaseDeDatosTests : IDisposable
    {
        private readonly SqliteConnection _conexion;
        private readonly DbContextOptions<FerreteriaDbContext> _opciones;

        protected BaseDeDatosTests()
        {
            // La base en memoria de SQLite existe solo mientras la conexión esté abierta.
            _conexion = new SqliteConnection("DataSource=:memory:");
            _conexion.Open();

            _opciones = new DbContextOptionsBuilder<FerreteriaDbContext>()
                .UseSqlite(_conexion)
                .Options;

            using var contexto = NuevoContexto();
            contexto.Database.EnsureCreated();
        }

        protected FerreteriaDbContext NuevoContexto() => new(_opciones);

        public void Dispose() => _conexion.Dispose();
    }
}
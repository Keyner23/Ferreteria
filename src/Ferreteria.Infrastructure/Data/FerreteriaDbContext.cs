using Ferreteria.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ferreteria.Infrastructure.Data
{
    public class FerreteriaDbContext : DbContext
    {
        public FerreteriaDbContext(DbContextOptions<FerreteriaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasKey(u => u.Id);

                e.Property(u => u.Correo).IsRequired().HasMaxLength(150);
                e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
                e.Property(u => u.Rol).IsRequired().HasConversion<int>();

                e.HasIndex(u => u.Correo).IsUnique();

                e.HasOne(u => u.Cliente)
                 .WithOne(c => c.Usuario)
                 .HasForeignKey<Cliente>(c => c.UsuarioId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Cliente>(e =>
            {
                e.HasKey(c => c.Id);

                e.Property(c => c.Nombre).IsRequired().HasMaxLength(50);
                e.Property(c => c.Apellido).IsRequired().HasMaxLength(50);
                e.Property(c => c.Documento).IsRequired().HasMaxLength(20);
                e.Property(c => c.Telefono).HasMaxLength(20);
                e.Property(c => c.Direccion).HasMaxLength(200);

                e.HasIndex(c => c.Documento).IsUnique();
            });

            modelBuilder.Entity<Categoria>(e =>
            {
                e.HasKey(c => c.Id);

                e.Property(c => c.Nombre).IsRequired().HasMaxLength(60);

                e.HasIndex(c => c.Nombre).IsUnique();
            });

            modelBuilder.Entity<Producto>(e =>
            {
                e.HasKey(p => p.Id);

                e.Property(p => p.Codigo).IsRequired().HasMaxLength(30);
                e.Property(p => p.Nombre).IsRequired().HasMaxLength(120);
                e.Property(p => p.Descripcion).HasMaxLength(500);
                e.Property(p => p.PrecioUnitario).HasColumnType("decimal(18,2)");

                e.HasIndex(p => p.Codigo).IsUnique();

                e.HasOne(p => p.Categoria)
                 .WithMany(c => c.Productos)
                 .HasForeignKey(p => p.CategoriaId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Venta>(e =>
            {
                e.HasKey(v => v.Id);

               

                e.HasOne(v => v.Cliente)
                 .WithMany(c => c.Ventas)
                 .HasForeignKey(v => v.ClienteId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DetalleVenta>(e =>
            {
                e.HasKey(d => d.Id);

                e.Property(d => d.NombreProducto).IsRequired().HasMaxLength(120);
                e.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");

                e.Ignore(d => d.Subtotal);

                e.HasOne(d => d.Venta)
                 .WithMany(v => v.Detalles)
                 .HasForeignKey(d => d.VentaId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(d => d.Producto)
                 .WithMany()
                 .HasForeignKey(d => d.ProductoId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

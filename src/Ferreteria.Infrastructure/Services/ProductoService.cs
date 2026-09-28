using Ferreteria.Application.Exceptions;
using Ferreteria.Application.Interfaces;
using Ferreteria.Domain.Entities;
using Ferreteria.Infrastructure.Data;
using Ferreteria.Shared.Dtos.Productos;
using Microsoft.EntityFrameworkCore;


namespace Ferreteria.Infrastructure.Services
{
    public class ProductoService : IProductoService
    {
        private readonly FerreteriaDbContext _context;

        public ProductoService(FerreteriaDbContext context)
        {
            _context = context;
        }

        // Proyección compartida: una sola definición de cómo un Producto se vuelve DTO.
        private static readonly System.Linq.Expressions.Expression<Func<Producto, ProductoDto>> ADto =
            p => new ProductoDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioUnitario = p.PrecioUnitario,
                Stock = p.Stock,
                Activo = p.Activo,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria!.Nombre
            };

        public async Task<List<ProductoDto>> ObtenerTodosAsync(bool soloActivos = true)
        {
            var consulta = _context.Productos.AsNoTracking();

            if (soloActivos)
                consulta = consulta.Where(p => p.Activo);

            return await consulta.OrderBy(p => p.Nombre)
                                 .Select(ADto)
                                 .ToListAsync();
        }

        public async Task<ProductoDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos
                                 .AsNoTracking()
                                 .Where(p => p.Id == id)
                                 .Select(ADto)
                                 .FirstOrDefaultAsync();
        }

        public async Task<ProductoDto> CrearAsync(ProductoRequest request)
        {
            var codigo = request.Codigo.Trim().ToUpperInvariant();

            await ValidarCategoriaAsync(request.CategoriaId);

            if (await _context.Productos.AnyAsync(p => p.Codigo == codigo))
                throw new ConflictException($"Ya existe un producto con el código {codigo}.");

            var producto = new Producto
            {
                Codigo = codigo,
                Nombre = request.Nombre.Trim(),
                Descripcion = request.Descripcion?.Trim(),
                PrecioUnitario = request.PrecioUnitario,
                Stock = request.Stock,
                CategoriaId = request.CategoriaId,
                Activo = request.Activo
            };

            _context.Productos.Add(producto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException($"Ya existe un producto con el código {codigo}.");
            }

            return (await ObtenerPorIdAsync(producto.Id))!;
        }

        public async Task ActualizarAsync(int id, ProductoRequest request)
        {
            var producto = await _context.Productos.FindAsync(id)
                ?? throw new NotFoundException($"No existe un producto con Id {id}.");

            var codigo = request.Codigo.Trim().ToUpperInvariant();

            await ValidarCategoriaAsync(request.CategoriaId);

            if (await _context.Productos.AnyAsync(p => p.Codigo == codigo && p.Id != id))
                throw new ConflictException($"Ya existe otro producto con el código {codigo}.");

            producto.Codigo = codigo;
            producto.Nombre = request.Nombre.Trim();
            producto.Descripcion = request.Descripcion?.Trim();
            producto.PrecioUnitario = request.PrecioUnitario;
            producto.Stock = request.Stock;
            producto.CategoriaId = request.CategoriaId;
            producto.Activo = request.Activo;

            await _context.SaveChangesAsync();
        }

        public async Task DesactivarAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id)
                ?? throw new NotFoundException($"No existe un producto con Id {id}.");

            producto.Activo = false;

            await _context.SaveChangesAsync();
        }

        private async Task ValidarCategoriaAsync(int categoriaId)
        {
            if (!await _context.Categorias.AnyAsync(c => c.Id == categoriaId))
                throw new NotFoundException($"No existe una categoría con Id {categoriaId}.");
        }
    }
}

using Ferreteria.Application.Exceptions;
using Ferreteria.Application.Interfaces;
using Ferreteria.Domain.Entities;
using Ferreteria.Infrastructure.Data;
using Ferreteria.Shared.Dtos.Productos;
using Microsoft.EntityFrameworkCore;



namespace Ferreteria.Infrastructure.Services
{

    public class CategoriaService : ICategoriaService
    {
        private readonly FerreteriaDbContext _context;

        public CategoriaService(FerreteriaDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoriaDto>> ObtenerTodasAsync()
        {
            return await _context.Categorias
                                 .AsNoTracking()
                                 .OrderBy(c => c.Nombre)
                                 .Select(c => new CategoriaDto { Id = c.Id, Nombre = c.Nombre })
                                 .ToListAsync();
        }

        public async Task<CategoriaDto> CrearAsync(CategoriaRequest request)
        {
            var nombre = request.Nombre.Trim();

            if (await _context.Categorias.AnyAsync(c => c.Nombre == nombre))
                throw new ConflictException($"Ya existe una categoría llamada '{nombre}'.");

            var categoria = new Categoria { Nombre = nombre };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return new CategoriaDto { Id = categoria.Id, Nombre = categoria.Nombre };
        }

        public async Task ActualizarAsync(int id, CategoriaRequest request)
        {
            var categoria = await _context.Categorias.FindAsync(id)
                ?? throw new NotFoundException($"No existe una categoría con Id {id}.");

            var nombre = request.Nombre.Trim();

            if (await _context.Categorias.AnyAsync(c => c.Nombre == nombre && c.Id != id))
                throw new ConflictException($"Ya existe otra categoría llamada '{nombre}'.");

            categoria.Nombre = nombre;

            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id)
                ?? throw new NotFoundException($"No existe una categoría con Id {id}.");

            if (await _context.Productos.AnyAsync(p => p.CategoriaId == id))
                throw new ConflictException(
                    "No se puede eliminar una categoría que tiene productos. Muévelos primero.");

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }
    }

}

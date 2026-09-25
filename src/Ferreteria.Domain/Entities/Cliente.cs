
namespace Ferreteria.Domain.Entities
{
    public class Cliente
    {
        public int Id { get; set; }

        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; } = true;

        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}

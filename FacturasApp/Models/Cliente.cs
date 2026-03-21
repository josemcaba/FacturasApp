namespace FacturasApp.Models
{
    public class Cliente
    {
        public string Nombre { get; set; } = string.Empty;
        public string NIF { get; set; } = string.Empty;
        public string? Direccion { get; set; }
    }
}

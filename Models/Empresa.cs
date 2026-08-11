namespace FacturasApp.Models
{
    public class Empresa
    {
        private string _nombre = string.Empty;
        public string NIF { get; set; } = string.Empty;

        public string Nombre
        {
            get => _nombre;
            set => _nombre = value.ToUpperInvariant();
        }
    }
}

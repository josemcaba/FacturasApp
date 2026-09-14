namespace FacturasApp.Core.Models
{
    public class PlantillaOcr
    {
        public string Emisor { get; set; } = string.Empty;
        public List<ZonaOcr> Zonas { get; set; } = new();
    }

    public class ZonaOcr
    {
        public string Campo { get; set; } = string.Empty;
        public int NumPagina { get; set; } = 1;
        public double X { get; set; }
        public double Y { get; set; }
        public double Ancho { get; set; }
        public double Alto { get; set; }
        public PreprocesamientoOcr Preprocesamiento { get; set; } = new();
    }
}

using System.Xml.Serialization;

namespace FacturasApp.Models
{
    // Representa una zona rectangular en la factura
    [XmlRoot("PlantillasOcr")]
    public class PlantillasOcrColeccion
    {
        [XmlElement("Plantilla")]
        public List<PlantillaOcr> Plantillas { get; set; } = new();
    }

    public class PlantillaOcr
    {
        // Nombre del emisor — coincide con parser.Nombre
        [XmlAttribute("Emisor")]
        public string Emisor { get; set; } = string.Empty;

        [XmlElement("Zona")]
        public List<ZonaOcr> Zonas { get; set; } = new();
    }

    public class ZonaOcr
    {
        // Nombre del campo que contiene esta zona
        // Debe coincidir con los campos de Factura
        [XmlAttribute("Campo")]
        public string Campo { get; set; } = string.Empty;

        // Coordenadas en porcentaje sobre el tamaño de la página
        // Así funcionan con cualquier resolución de escaneo
        [XmlAttribute("X")]
        public double X { get; set; }

        [XmlAttribute("Y")]
        public double Y { get; set; }

        [XmlAttribute("Ancho")]
        public double Ancho { get; set; }

        [XmlAttribute("Alto")]
        public double Alto { get; set; }

        // Convierte las coordenadas porcentuales a píxeles
        // según el tamaño real de la imagen
        public System.Drawing.Rectangle ToRectangle(int imgAncho, int imgAlto)
        {
            return new System.Drawing.Rectangle(
                (int)(X * imgAncho / 100.0),
                (int)(Y * imgAlto / 100.0),
                (int)(Ancho * imgAncho / 100.0),
                (int)(Alto * imgAlto / 100.0));
        }
    }
}

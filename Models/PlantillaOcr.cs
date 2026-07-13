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

        // NUEVAS PROPIEDADES (con valores por defecto para compatibilidad)
        public double AnchoReferencia { get; set; } = 1000;
        public double AltoReferencia { get; set; } = 1000;
        public TipoCoordenadas TipoCoordenada { get; set; } = TipoCoordenadas.Normalizadas;
    }

    public enum TipoCoordenadas
    {
        Normalizadas = 0,    // 0-1 (comportamiento actual)
        AbsolutasPuntos = 1,
        AbsolutasPixels = 2
    }

    public class ZonaOcr
    {
        // Nombre del campo que contiene esta zona
        [XmlAttribute("Campo")]
        public string Campo { get; set; } = string.Empty;

        // Número de página a la que pertenece esta zona (1-based)
        [XmlAttribute("Pagina")]
        public int NumPagina { get; set; } = 1;

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

        [XmlAttribute("Regex")]
        public string RegexPersonalizada { get; set; } = string.Empty;

        // NUEVAS PROPIEDADES
        public string? RegexRespaldo { get; set; }
        public bool Opcional { get; set; } = false;
        public PreprocesamientoOcr Preprocesamiento { get; set; } = new();

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



        public string ExtraerConRespaldo(string textoZonaDirecto, string? textoCompleto = null)
        {
            if (!string.IsNullOrWhiteSpace(textoZonaDirecto))
                return textoZonaDirecto.Trim();

            if (Opcional)
                return string.Empty;

            if (!string.IsNullOrEmpty(RegexRespaldo) && !string.IsNullOrEmpty(textoCompleto))
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    textoCompleto,
                    RegexRespaldo,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (match.Success)
                    return match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : match.Value.Trim();
            }

            return string.Empty;
        }
    }
}

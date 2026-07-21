using System.Xml.Serialization;

namespace FacturasApp.Models
{
    public class MultiFacturaConfig
    {
        [XmlElement("LineaIva")]
        public LineaIvaConfig? LineaIva { get; set; }

        [XmlElement("ValidarSuma")]
        public bool ValidarSuma { get; set; } = true;

        [XmlElement("FiltrarBaseCero")]
        public bool FiltrarBaseCero { get; set; } = true;

        [XmlElement("Deduplicar")]
        public bool Deduplicar { get; set; } = true;
    }

    public class LineaIvaConfig
    {
        [XmlElement("Regex")]
        public string Regex { get; set; } = string.Empty;

        [XmlArray("Mapeo")]
        [XmlArrayItem("Asignar")]
        public List<AsignacionCampo> Mapeo { get; set; } = new();
    }

    public class AsignacionCampo
    {
        [XmlAttribute("Grupo")]
        public int Grupo { get; set; }

        [XmlAttribute("Campo")]
        public string Campo { get; set; } = string.Empty;
    }
}

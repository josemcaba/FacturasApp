using System.Xml.Serialization;

namespace FacturasApp.Models
{
    public class CampoExtraccion
    {
        [XmlAttribute("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [XmlAttribute("tipo")]
        public string Tipo { get; set; } = "Texto";

        [XmlElement("Regex")]
        public string? Regex { get; set; }

        [XmlElement("Grupo")]
        public int Grupo { get; set; } = 1;

        [XmlElement("ValorFijo")]
        public string? ValorFijo { get; set; }

        [XmlElement("FormatoFecha")]
        public string? FormatoFecha { get; set; }

        [XmlArray("FormatosFecha")]
        [XmlArrayItem("Formato")]
        public List<string> FormatosFecha { get; set; } = new();

        [XmlElement("Cultura")]
        public string Cultura { get; set; } = "es-ES";

        [XmlElement("Opcional")]
        public bool Opcional { get; set; } = false;
    }
}

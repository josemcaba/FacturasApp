using System.Xml.Serialization;

namespace FacturasApp.Models
{
    public class ReglaPostProcesamiento
    {
        [XmlAttribute("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [XmlElement("Condicion")]
        public CondicionRegla? Condicion { get; set; }

        [XmlElement("Accion")]
        public List<AccionRegla> Acciones { get; set; } = new();
    }

    public class CondicionRegla
    {
        [XmlAttribute("Campo")]
        public string? Campo { get; set; }

        [XmlAttribute("Operador")]
        public string Operador { get; set; } = "Igual";

        [XmlAttribute("Valor")]
        public string Valor { get; set; } = string.Empty;

        [XmlAttribute("TextoContiene")]
        public string? TextoContiene { get; set; }
    }

    public class AccionRegla
    {
        [XmlAttribute("tipo")]
        public string Tipo { get; set; } = "SetValor";

        [XmlAttribute("Campo")]
        public string Campo { get; set; } = string.Empty;

        [XmlAttribute("Valor")]
        public string Valor { get; set; } = string.Empty;

        [XmlAttribute("CampoFuente")]
        public string? CampoFuente { get; set; }
    }
}

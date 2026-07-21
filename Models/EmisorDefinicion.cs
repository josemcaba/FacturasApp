using System.Xml.Serialization;

namespace FacturasApp.Models
{
    public class EmisorDefinicion
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = string.Empty;

        [XmlElement("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [XmlElement("Nif")]
        public string Nif { get; set; } = string.Empty;

        [XmlElement("Concepto")]
        public string Concepto { get; set; } = "600";

        [XmlArray("Identificadores")]
        [XmlArrayItem("Identificador")]
        public List<string> Identificadores { get; set; } = new();

        [XmlElement("ModoExtraccion")]
        public string ModoExtraccion { get; set; } = "OrdenadoPosicion";

        [XmlArray("PreProcesamiento")]
        [XmlArrayItem("Reemplazar")]
        public List<ReglaPreProcesamiento> PreProcesamiento { get; set; } = new();

        [XmlArray("Campos")]
        [XmlArrayItem("Campo")]
        public List<CampoExtraccion> Campos { get; set; } = new();

        [XmlElement("MultiFactura")]
        public MultiFacturaConfig? MultiFactura { get; set; }

        [XmlArray("PostProcesamiento")]
        [XmlArrayItem("Regla")]
        public List<ReglaPostProcesamiento> PostProcesamiento { get; set; } = new();

        [XmlElement("ZonasOcr")]
        public ZonasOcrConfig? ZonasOcr { get; set; }
    }
}

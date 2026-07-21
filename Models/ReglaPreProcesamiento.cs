using System.Xml.Serialization;

namespace FacturasApp.Models
{
    public class ReglaPreProcesamiento
    {
        [XmlAttribute("busco")]
        public string Busco { get; set; } = string.Empty;

        [XmlAttribute("por")]
        public string Por { get; set; } = string.Empty;

        [XmlAttribute("regex")]
        public bool EsRegex { get; set; } = false;
    }
}

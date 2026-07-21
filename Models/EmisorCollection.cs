using System.Xml.Serialization;

namespace FacturasApp.Models
{
    [XmlRoot("Emisores")]
    public class EmisorCollection
    {
        [XmlElement("Emisor")]
        public List<EmisorDefinicion> Emisores { get; set; } = new();
    }
}

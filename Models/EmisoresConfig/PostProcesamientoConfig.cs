using System.Xml.Serialization;

namespace FacturasApp.Models.EmisoresConfig;

public class PostProcesamientoConfig
{
    [XmlAttribute("Tipo")]
    public string Tipo { get; set; } = string.Empty;

    public string? CondicionTextoContiene { get; set; }

    [XmlArray("CamposAfectados")]
    [XmlArrayItem("Campo")]
    public List<string> CamposAfectados { get; set; } = new();
}

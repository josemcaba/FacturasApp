using System.Xml.Serialization;

namespace FacturasApp.Models.EmisoresConfig;

public class CampoConfig
{
    [XmlAttribute("Nombre")]
    public string Nombre { get; set; } = string.Empty;

    public string? Regex { get; set; }

    public string? ValorFijo { get; set; }
    public bool UsarRegexFechaGeneral { get; set; }
    public bool UsarRegexNifGeneral { get; set; }
    public bool EsSuma { get; set; }

    [XmlArray("CamposSuma")]
    [XmlArrayItem("Campo")]
    public List<string>? CamposSuma { get; set; }

    public string? FormatoFecha { get; set; }
}

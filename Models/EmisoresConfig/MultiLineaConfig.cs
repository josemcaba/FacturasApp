using System.Xml.Serialization;

namespace FacturasApp.Models.EmisoresConfig;

public class MultiLineaConfig
{
    public bool Habilitado { get; set; }

    // Compatibilidad: si Lineas está vacía, se usa RegexLinea + MapeoCampos
    public string RegexLinea { get; set; } = string.Empty;

    [XmlArray("Lineas")]
    [XmlArrayItem("Linea")]
    public List<LineaConfig> Lineas { get; set; } = new();

    [XmlArray("MapeoCampos")]
    [XmlArrayItem("Mapeo")]
    public List<MapeoCampoLinea> MapeoCampos { get; set; } = new();
}

public class LineaConfig
{
    public string Regex { get; set; } = string.Empty;

    [XmlArray("MapeoCampos")]
    [XmlArrayItem("Mapeo")]
    public List<MapeoCampoLinea> MapeoCampos { get; set; } = new();
}

public class MapeoCampoLinea
{
    [XmlAttribute("Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [XmlAttribute("Grupo")]
    public int Grupo { get; set; }
}

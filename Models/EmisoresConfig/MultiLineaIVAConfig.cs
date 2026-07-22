using System.Xml.Serialization;

namespace FacturasApp.Models.EmisoresConfig;

public class MultiLineaIVAConfig
{
    public bool Habilitado { get; set; }
    public string RegexLinea { get; set; } = string.Empty;

    [XmlArray("MapeoCampos")]
    [XmlArrayItem("Mapeo")]
    public List<MapeoCampoMultiIVA> MapeoCampos { get; set; } = new();
}

public class MapeoCampoMultiIVA
{
    [XmlAttribute("Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [XmlAttribute("Grupo")]
    public int Grupo { get; set; }
}

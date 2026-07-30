using System.Xml.Serialization;

namespace FacturasApp.Models.EmisoresConfig;

public class ZonaOcrConfig
{
    [XmlAttribute("Campo")]
    public string Campo { get; set; } = string.Empty;

    [XmlAttribute("Pagina")]
    public int NumPagina { get; set; } = 1;

    [XmlAttribute("X")]
    public double X { get; set; }

    [XmlAttribute("Y")]
    public double Y { get; set; }

    [XmlAttribute("Ancho")]
    public double Ancho { get; set; }

    [XmlAttribute("Alto")]
    public double Alto { get; set; }

    public PreprocesamientoOcr? Preprocesamiento { get; set; }
}

using System.Xml.Serialization;

namespace FacturasApp.Models.EmisoresConfig;

[XmlRoot("Emisor")]
public class EmisorConfig
{
    public string Nif { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    [XmlArray("Identificadores")]
    [XmlArrayItem("Id")]
    public List<string> Identificadores { get; set; } = new();

    public string ModoExtraccion { get; set; } = "OrdenadoPosicion";
    public string ConceptoIngreso { get; set; } = "700";
    public string ConceptoGasto { get; set; } = "600";
    public string CulturaFecha { get; set; } = "es-ES";
    public string RutaPdfMuestra { get; set; } = string.Empty;

    [XmlArray("Campos")]
    [XmlArrayItem("Campo")]
    public List<CampoConfig> Campos { get; set; } = new();

    public MultiLineaConfig? MultiLinea { get; set; }

    [XmlArray("PostProcesamiento")]
    [XmlArrayItem("Regla")]
    public List<PostProcesamientoConfig> PostProcesamiento { get; set; } = new();

    [XmlArray("ZonasOcr")]
    [XmlArrayItem("Zona")]
    public List<ZonaOcrConfig>? ZonasOcr { get; set; }
}

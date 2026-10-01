using System.Xml.Serialization;

namespace FacturasApp.Core.Models.EmisoresConfig;

[XmlRoot("Emisor")]
public class EmisorConfig
{
    public string Nif { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Versión del emisor como timestamp yyyyMMddHHmm. Va declarado tras Nombre
    /// para que el orden serializado coincida con el XML editado a mano.
    /// </summary>
    /// <remarks>
    /// Se toma <c>long</c>, no <c>int</c>: un timestamp como 202610011530 supera
    /// Int32.MaxValue (2.147.483.647) y desbordaría a negativo, dejando el emisor
    /// congelado por debajo de cualquier versión publicada.
    /// Un XML sin &lt;Version&gt; se lee como 1, siempre menor que cualquier timestamp.
    /// </remarks>
    public long Version { get; set; } = 1;

    [XmlArray("Identificadores")]
    [XmlArrayItem("Id")]
    public List<string> Identificadores { get; set; } = new();

    public string ModoExtraccion { get; set; } = "Ordenado";
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

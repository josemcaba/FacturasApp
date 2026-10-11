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

    /// <summary>Momento de la última consulta al servicio VIES (null = nunca
    /// verificado).</summary>
    /// <remarks>
    /// Se serializa vía <see cref="ViesFechaVerificacionTexto"/>: XmlSerializer
    /// escribe un &lt;xsi:nil&gt; para los DateTime? vacíos, lo que añadiría ruido
    /// a los XML de los emisores sin verificar.
    /// </remarks>
    [XmlIgnore]
    public DateTime? ViesFechaVerificacion { get; set; }

    /// <summary>Serialización de <see cref="ViesFechaVerificacion"/> como texto
    /// ISO. Un string null no se serializa, así que el elemento sólo aparece
    /// cuando hay una verificación real guardada.</summary>
    [XmlElement("ViesFechaVerificacion")]
    public string? ViesFechaVerificacionTexto
    {
        get => ViesFechaVerificacion?.ToString("yyyy-MM-ddTHH:mm:ss",
            System.Globalization.CultureInfo.InvariantCulture);
        set => ViesFechaVerificacion =
            DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var fecha) ? fecha : null;
    }

    /// <summary>Resultado de la última consulta VIES: "Valido", "Invalido" o
    /// "NoDisponible" (null = nunca verificado).</summary>
    public string? ViesEstado { get; set; }
}

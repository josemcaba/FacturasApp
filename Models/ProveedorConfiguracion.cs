using System.Xml.Serialization;

namespace FacturasApp.Models
{
    [XmlRoot("Proveedores")]
    public class ProveedoresConfiguracion
    {
        [XmlElement("Proveedor")]
        public List<ProveedorConfig> Proveedores { get; set; } = new();
    }

    public class ProveedorConfig
    {
        [XmlAttribute("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [XmlAttribute("Nif")]
        public string Nif { get; set; } = string.Empty;

        [XmlAttribute("Concepto")]
        public string Concepto { get; set; } = "600";

        [XmlAttribute("ModoExtraccion")]
        public ModoExtraccionTexto ModoExtraccion { get; set; } = ModoExtraccionTexto.OrdenadoPosicion;

        [XmlArray("Identificadores")]
        [XmlArrayItem("Id")]
        public List<string> Identificadores { get; set; } = new();

        public PreprocesamientoConfig Preprocesamiento { get; set; } = new();

        [XmlArray("Campos")]
        [XmlArrayItem("Campo")]
        public List<CampoConfig> Campos { get; set; } = new();

        public MultiLineaIvaConfig? MultiLineaIva { get; set; }

        public PostprocesamientoConfig Postprocesamiento { get; set; } = new();

        public bool DebeOmitirNifEmisor { get; set; } = true;
    }

    public enum ModoExtraccionTexto
    {
        Simple,
        OrdenadoPosicion,
        LayoutAnalysis
    }

    public class PreprocesamientoConfig
    {
        [XmlElement("Reemplazar")]
        public List<ReemplazoConfig> Reemplazos { get; set; } = new();

        [XmlElement("EliminarDuplicados")]
        public List<EliminarDuplicadosConfig> EliminarDuplicados { get; set; } = new();
    }

    public class ReemplazoConfig
    {
        [XmlAttribute("Pattern")]
        public string Pattern { get; set; } = string.Empty;

        [XmlAttribute("Reemplazo")]
        public string Reemplazo { get; set; } = string.Empty;
    }

    public class EliminarDuplicadosConfig
    {
        [XmlAttribute("Tipo")]
        public TipoEliminarDuplicados Tipo { get; set; } = TipoEliminarDuplicados.NoNumericos;
    }

    public enum TipoEliminarDuplicados
    {
        Numericos,
        NoNumericos
    }

    public class CampoConfig
    {
        [XmlAttribute("Nombre")]
        public CampoFactura Nombre { get; set; }

        [XmlAttribute("Regex")]
        public string Regex { get; set; } = string.Empty;

        [XmlAttribute("Grupo")]
        public int Grupo { get; set; } = 1;

        [XmlAttribute("ValorFijo")]
        public string ValorFijo { get; set; } = string.Empty;

        [XmlAttribute("Cultura")]
        public string Cultura { get; set; } = "es-ES";

        [XmlAttribute("Formato")]
        public string Formato { get; set; } = string.Empty;

        [XmlAttribute("Opcional")]
        public bool Opcional { get; set; } = false;

        [XmlAttribute("DeduplicarNumericos")]
        public bool DeduplicarNumericos { get; set; } = false;
    }

    public enum CampoFactura
    {
        NumeroFactura,
        Fecha,
        ClienteNombre,
        ClienteNif,
        BaseImponible,
        PorcentajeIVA,
        CuotaIVA,
        PorcentajeIRPF,
        CuotaIRPF,
        PorcentajeRE,
        CuotaRE,
        Total,
        Descuento,
        TotalParcial
    }

    public class MultiLineaIvaConfig
    {
        [XmlElement("Linea")]
        public List<LineaIvaConfig> Lineas { get; set; } = new();

        public TotalFacturaConfig? TotalFactura { get; set; }

        [XmlElement("Deduplicar")]
        public bool Deduplicar { get; set; } = false;

        [XmlElement("ExcluirBaseCero")]
        public bool ExcluirBaseCero { get; set; } = false;

        [XmlElement("ValidarSumaSubtotales")]
        public bool ValidarSumaSubtotales { get; set; } = false;

        [XmlElement("CrearFacturaPorLinea")]
        public bool CrearFacturaPorLinea { get; set; } = true;
    }

    public class LineaIvaConfig
    {
        [XmlAttribute("Regex")]
        public string Regex { get; set; } = string.Empty;

        [XmlAttribute("Mapa")]
        public string Mapa { get; set; } = string.Empty;
    }

    public class TotalFacturaConfig
    {
        [XmlAttribute("Regex")]
        public string Regex { get; set; } = string.Empty;

        [XmlAttribute("Grupo")]
        public int Grupo { get; set; } = 1;
    }

    public class PostprocesamientoConfig
    {
        [XmlElement("Condicion")]
        public List<CondicionConfig> Condiciones { get; set; } = new();
    }

    public class CondicionConfig
    {
        [XmlAttribute("Campo")]
        public string Campo { get; set; } = string.Empty;

        [XmlAttribute("Valor")]
        public string Valor { get; set; } = string.Empty;

        [XmlAttribute("Operador")]
        public OperadorCondicion Operador { get; set; } = OperadorCondicion.Igual;

        [XmlElement("MoverCampo")]
        public List<MoverCampoConfig> MoverCampos { get; set; } = new();

        [XmlElement("AsignarValorFijo")]
        public List<AsignarValorFijoConfig> AsignarValoresFijos { get; set; } = new();

        [XmlElement("CopiarCampo")]
        public List<CopiarCampoConfig> CopiarCampos { get; set; } = new();

        [XmlElement("SumarCampo")]
        public List<SumarCampoConfig> SumarCampos { get; set; } = new();
    }

    public enum OperadorCondicion
    {
        Igual,
        Distinto,
        MayorQue,
        MenorQue,
        MayorOIgual,
        MenorOIgual
    }

    public class MoverCampoConfig
    {
        [XmlAttribute("Origen")]
        public string Origen { get; set; } = string.Empty;

        [XmlAttribute("Destino")]
        public string Destino { get; set; } = string.Empty;
    }

    public class AsignarValorFijoConfig
    {
        [XmlAttribute("Campo")]
        public string Campo { get; set; } = string.Empty;

        [XmlAttribute("Valor")]
        public string Valor { get; set; } = string.Empty;
    }

    public class CopiarCampoConfig
    {
        [XmlAttribute("Origen")]
        public string Origen { get; set; } = string.Empty;

        [XmlAttribute("Destino")]
        public string Destino { get; set; } = string.Empty;
    }

    public class SumarCampoConfig
    {
        [XmlAttribute("Destino")]
        public string Destino { get; set; } = string.Empty;

        [XmlAttribute("Origen")]
        public string Origen { get; set; } = string.Empty;
    }
}

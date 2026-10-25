using System.Xml.Serialization;

namespace FacturasApp.Core.Models
{
    [XmlRoot("ColumnasExcel")]
    public class ColumnasExcelConfig
    {
        /// <summary>
        /// Timestamp de publicación (yyyyMMddHHmm). Un XML sin &lt;Version&gt; se lee como 1.
        /// </summary>
        public long Version { get; set; } = 1;

        [XmlArray("Columnas")]
        [XmlArrayItem("Columna")]
        public List<ColumnaExcelConfig> Columnas { get; set; } = new();
    }

    public class ColumnaExcelConfig
    {
        /// <summary>
        /// Campo interno fijo (NumeroFactura, Fecha, BaseImponible, ...).
        /// </summary>
        public string Campo { get; set; } = "";

        [XmlArray("Cabeceras")]
        [XmlArrayItem("Cabecera")]
        public List<string> Cabeceras { get; set; } = new();
    }
}
using System.Text.RegularExpressions;
using FacturasApp.Models;
using FacturasApp.Services;

namespace FacturasApp.Services.Parsers
{
    public abstract class BaseParser : IInvoiceParser
    {
        public abstract string Nombre { get; }
        public abstract bool PuedeParsar(string texto);
        public abstract Factura Parsear(string texto, string rutaArchivo, bool viaOcr);

        // Sobreescribe en el parser específico si necesita otro modo
        public virtual PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.LayoutAnalysis;

        protected string ExtraerGrupo(Regex regex, string texto, int grupo)
        {
            var m = regex.Match(texto);
            return m.Success ? m.Groups[grupo].Value.Trim() : string.Empty;
        }

        protected decimal ExtraerDecimal(Regex regex, string texto, int grupo)
        {
            var m = regex.Match(texto);
            if (!m.Success) return 0m;
            string val = m.Groups[grupo].Value
                .Replace(".", "")
                .Replace(",", ".");
            return decimal.TryParse(val,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var r) ? r : 0m;
        }

        protected EstadoFactura DeterminarEstado(Factura f) =>
            !string.IsNullOrEmpty(f.NumeroFactura) && f.Fecha.HasValue && f.Total > 0
                ? EstadoFactura.OK
                : EstadoFactura.RevisionManual;
    }
}
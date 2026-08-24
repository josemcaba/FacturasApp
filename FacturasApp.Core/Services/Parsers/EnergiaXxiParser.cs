using DocumentFormat.OpenXml.Presentation;
using FacturasApp.Core.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class EnergiaXxiParser: BaseParser
    {
        public override string Nombre => "ENERGIA XXI COMERCIALIZADORA S.L.U.";
        public override string Nif => "B82846825";  

        protected override string[] Identificadores =>
            ["B82846825", "comercializadora"];

        private static readonly Regex RegexNumero = new(
            @"N.factura:\s+(\S+)",
            RegexOptions.Compiled);
        

        private static readonly Regex RegexReceptorNombre = new(
            @"Titular del contrato:\s+(.+)?NIF",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"IVA normal:\s+([\d,.]+)\s*%\s+s\/\s+([\d,.]+)\s+.+?\s+(.+)?€",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"TOTAL IMPORTE FACTURA\s+([\d,.]+)",
            RegexOptions.Compiled);

        protected override Regex RegexFecha { get; } = new(
            @"emitida el\s+(.+)",
            RegexOptions.Compiled);

        private DateTime? ExtraerFechaEspañol(string texto)
        {
            var m = RegexFecha.Match(texto);
            if (!m.Success) return null;

            string fechaStr = m.Groups[1].Value.Trim();
            string[] formatos = ["d 'de' MMMM 'de' yyyy", "d 'de' MMM 'de' yyyy"];

            return DateTime.TryParseExact(
                fechaStr, formatos,
                new CultureInfo("es-ES"),
                DateTimeStyles.None,
                out var fecha) ? fecha : null;
        }
        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFechaEspañol(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 2);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 1);

            string CuotaIVA = RegexImportes.Match(texto).Groups[3].Value;
            CuotaIVA = Regex.Replace(CuotaIVA, "[ .]", "");
            factura.CuotaIVA = ParsearDecimal(CuotaIVA);
            
            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

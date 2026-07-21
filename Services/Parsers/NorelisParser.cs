using DocumentFormat.OpenXml.Presentation;
using FacturasApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    [Obsolete("Usar emisores.xml en su lugar")]
    public class NorelisParser: BaseParser
    {
        public override string Nombre => "NORELIS ANDREA ESLAVA";
        public override string Nif => "Y7080956Y";  

        protected override string[] Identificadores =>
            ["Y7080956Y", "norelis", "3876"];

        private static readonly Regex RegexNumero = new(
            @"Factura[\n\r]+(.*)",
            RegexOptions.Compiled);
        
        private static readonly Regex RegexReceptorNombre = new(
            @"Eslava\s+(.*)",
            RegexOptions.Compiled);
        protected override Regex RegexFecha { get; } = new(
            @"F. Emisión:(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexBase = new(
            @"Base\s+(?:imponible|exenta)\s+([\d.,]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexIva = new(
            @"IVA\s+([\d.,]+)\s*%.*?[\d.,]+.*?([\d.,]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"Total:\s+([\d.,]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexBase, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexIva, texto, 1);
            factura.CuotaIVA = ExtraerDecimal(RegexIva, texto, 2);
            factura.Total = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

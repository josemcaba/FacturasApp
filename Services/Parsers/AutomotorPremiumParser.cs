using DocumentFormat.OpenXml.Presentation;
using FacturasApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    [Obsolete("Usar emisores.xml en su lugar")]
    public class AutomotorPremiumParser: BaseParser
    {
        public override string Nombre => "AUTOMOTOR PREMIUM, S.L.";
        public override string Nif => "B93036515";  

        protected override string[] Identificadores =>
            ["B93036515", "automotor", "premium", "bmw"];

        private static readonly Regex RegexNumero = new(
            @"FACTURA[\n\r]+(.*)",
            RegexOptions.Compiled);
        
        private static readonly Regex RegexReceptorNombre = new(
            @"\[P1_Z2]:\s+(.*)",
            RegexOptions.Compiled);

        private static readonly Regex RegexBase = new(
            @"BASE IMPONIBLE\s+([\d.,]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexIva = new(
            @"IVA\s+([\d.,]+)\s*%\s+([\d.,]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"TOTAL FACTURA\s+([\d.,]+)",
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

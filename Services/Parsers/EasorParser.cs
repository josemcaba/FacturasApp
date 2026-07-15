using DocumentFormat.OpenXml.Presentation;
using FacturasApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class EasorParser: BaseParser
    {
        public override string Nombre => "EASOR SOFTWARE SOLUTIONS S.L.";
        public override string Nif => "B24830515";  

        protected override string[] Identificadores =>
            ["B24830515", "easor"];

        private static readonly Regex RegexNumero = new(
            @"Factura[\n\r]+(.*)",
            RegexOptions.Compiled);
        
        private static readonly Regex RegexReceptorNombre = new(
            @"SOFTWARE SOLUTIONS SL\s+(.*)",
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

            factura.Concepto = "629";
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

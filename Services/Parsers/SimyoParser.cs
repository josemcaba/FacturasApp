using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    [Obsolete("Usar emisores.xml en su lugar")]
    public class SimyoParser: BaseParser
    {
        public override string Nombre => "Orange España Virtual S.L.U.";
        public override string Nif => "B85057974";

        protected override string[] Identificadores =>
            ["simyo.es"];

        private static readonly Regex RegexNumero = new(
            @"MI FACTURA N.\s+(.*)",
            RegexOptions.Compiled);
        
        private static readonly Regex RegexReceptorNombre = new(
            @"Nombre titular:\s+(.*)",
            RegexOptions.Compiled);

        protected override Regex RegexFecha { get; } = new(
            @"Fecha de emisión:\s+(.*)",
            RegexOptions.Compiled);

        private static readonly Regex RegexBase = new(
            @"Base imponible\s+([\d.,]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"IVA\s+([\d.,]+)\s*%\s+([\d.,]+)[\S\n\s]+?([\d.,]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Concepto = "628";
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexBase, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 1);
            factura.CuotaIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.Total = ExtraerDecimal(RegexImportes, texto, 3);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

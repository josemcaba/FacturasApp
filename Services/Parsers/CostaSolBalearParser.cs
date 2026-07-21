using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    [Obsolete("Usar emisores.xml en su lugar")]
    public class CostaSolBalearParser: BaseParser
    {
        public override string Nombre => "COSTA DEL SOL BALEAR, S.L.";
        public override string Nif => "B92062603";  

        protected override string[] Identificadores =>
            ["costasol", "balear"];

        private static readonly Regex RegexNumero = new(
            @"DOCUMENTO[\n\r]+(.*)",
            RegexOptions.Compiled);
        
        private static readonly Regex RegexReceptorNombre = new(
            @"\[P1_Z1]:\s+(.*)",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"DOCUMENTO[\n\r]+[-\d.,]+\s+([-\d.,]+)\s+([-\d.,]+)\s+[-\d.,]+\s+([-\d.,]+)",
            RegexOptions.Compiled);

        protected override Regex RegexFecha { get; } = new(
            @"FECHA[\n\r]+(.*)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.Total = ExtraerDecimal(RegexImportes, texto, 3);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

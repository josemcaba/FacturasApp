
using FacturasApp.Core.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class TrigoricoParser: BaseParser
    {
        public override string Nombre => "TRIGORICO, S.L.U.";
        public override string Nif => "B87438750";

        protected override string[] Identificadores =>
            ["reinosa", "ESB87438750"];

        private static readonly Regex RegexNumero = new(
            @"(?:FACTURA|ABONO - RECTIFICATIVO|ABONO)\s+(\S+)",
            RegexOptions.Compiled);
        
        private static readonly Regex RegexReceptorNombre = new(
            @"(.*)[\n\r]+R.M. de Santander",
            RegexOptions.Compiled);

        protected override Regex RegexFecha { get; } = new(
            @"(?:FACTURA|ABONO - RECTIFICATIVO|ABONO)\s+\S+\s+(\S+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexRectificativo = new(
            @"ABONO",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"([\d.,]+)\s+([\d.,]+)\s+[\d.,]+\s+[\d.,]+[\n\r]+",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"Total Factura en Euro\s+([\d.,]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            decimal signo = RegexRectificativo.IsMatch(texto) ? -1 : 1;

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = signo * ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.TotalFactura = signo * ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

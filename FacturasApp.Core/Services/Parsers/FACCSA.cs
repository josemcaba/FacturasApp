using FacturasApp.Core.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class FACCSA : BaseParser
    {
        public override string Nombre => "FACCSA: Frigorif. And. Conservas Carne";
        public override string Nif => "A17001231";

        protected override string[] Identificadores =>
            ["andaluces", "conservas", "carne"];

        private static readonly Regex RegexNumero = new(
            @"\bFACTURA\s+(.*?)\s+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"CIF\s+.*?[\s\r\n]+(.*)\s",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _regexNif = new(
            @"CIF\s+(.*?)\s",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected override Regex RegexNif => _regexNif;

        private static readonly Regex RegexImportes = new(
            @"ASE\s+IMPONIB\.?\s+([\d.,]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"OTAL\s+FACTURA.*?([\d,.]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
            };

            factura.Emisor.NIF = Nif;
            factura.Emisor.Nombre = Nombre;
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(RegexFecha, texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre, texto, 1);
            factura.Receptor.NIF = ExtraerNif(RegexNif, texto, Nif);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = 10m;
            factura.PorcentajeRE = 1.4m;
            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}
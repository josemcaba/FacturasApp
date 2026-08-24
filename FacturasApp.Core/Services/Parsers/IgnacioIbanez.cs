using FacturasApp.Core.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class IgnacioIbanezParser : BaseParser
    {
        public override string Nombre => "Ignacio Ibañez Pacheco";
        public override string Nif => "33360360X";

        protected override string[] Identificadores =>
            ["SERVINFOTEC", "33.360.360-X"];

        private static readonly Regex RegexNumero = new(
            @"Número\s+([^\s]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"CLIENTE[\n\r]+(.+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _regexNif = new(
            @"(.{9,14})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected override Regex RegexNif => _regexNif;

        private static readonly Regex RegexImportes = new(
            @"([\d.,]+)\s+([\d.,]+)\s+([\d.,]+)\s+[\d.,]+\s+[\d.,]+",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"TOTAL[\r\n]+([\d.,]+)",
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
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.CuotaIVA = ExtraerDecimal(RegexImportes, texto, 3);
            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);
            
            return factura;
        }
    }
}

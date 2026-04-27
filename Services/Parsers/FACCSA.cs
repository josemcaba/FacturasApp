using CsvHelper;
using DocumentFormat.OpenXml.Vml;
using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class FACCSA : BaseParser
    {
        public override string Nombre => "FACCSA: Frigorif. And. Conservas Carne";
        public override string Nif => "A17001231";

        private static readonly string[] Identificadores =
            { "frigor", "ficos", "andaluces", "carne"};

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"\bFACTURA\s+(.*?)\s+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"CIF\s+.*?[\s\r\n]+(.*)\s",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"CIF\s+(.*?)\s",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"([\d,]+)\s+([\d,]+)\s+[\d,]+\s+([\d,]+)\s+[\d,]+\s",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"TOTAL\s+FACTURA.*?([\d,.]+)",
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
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.PorcentajeRE = ExtraerDecimal(RegexImportes, texto, 3);
            factura.Total = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = DeterminarEstado(factura);

            return factura;
        }
    }
}
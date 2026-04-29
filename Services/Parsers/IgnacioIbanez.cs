using CsvHelper;
using FacturasApp.Models;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class IgnacioIbanezParser : BaseParser
    {
        public override string Nombre => "Ignacio Ibañez Pacheco";
        public override string Nif => "33360360X";

        private static readonly string[] Identificadores =
            { "SERVINFOTEC", "33.360.360-X"};

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"Número\s+Fecha[\s\n\r]+([^\s]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"952\s*27\s*30\s*91[\s\n\r]+(.+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"\b(.+)[\r\n]+\d{6}\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"TOTAL[\n\r]+.*?%[\n\r\s]*(.+?)\s(.+?)\s(.+?)\s(.+?)\s(.+?)[\n\r\s]",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"\s(.+?)\s+Euros",
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
            factura.Total = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);
            
            return factura;
        }
    }
}

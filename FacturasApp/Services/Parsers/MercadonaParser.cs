using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class MercadonaParser : BaseParser
    {
        public override string Nombre => "Mercadona";

        private static readonly string[] Identificadores =
            { "MERCADONA S.A." };

        public override bool PuedeParsar(string texto) =>
            Identificadores.Any(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"N.\s*Factura:\s*(.*?)\s+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"Fecha\s*Factura:\s*(.*?)\s+",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"Razón Social: (.*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"NIF: ([A-Z]?\d{7,8}[A-Z]?)\s",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"(\d+)% ([\d,]+) ([\d,.]+) ([\d,.]+)",
            RegexOptions.Compiled);
        
        private static readonly Regex RegexTotal = new(
            @"Total Factura (.*)€",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
            };

            factura.Emisor.NIF = "A46103834";
            factura.Emisor.Nombre = "MERCADONA, S.A.";
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(RegexFecha, texto);
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre, texto, 1);
            factura.Receptor.NIF = ExtraerGrupo(RegexNif, texto, 1).ToUpper();
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 2);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 1);
            factura.Total = ExtraerDecimal(RegexImportes, texto, 4);
            factura.Estado = DeterminarEstado(factura);

            return factura;
        }
    }
}
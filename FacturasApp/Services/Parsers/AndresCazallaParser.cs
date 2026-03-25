using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class AndresCazallaParser : BaseParser
    {
        public override string Nombre => "Andrés Cazalla";

        private static readonly string[] Identificadores =
            { "cazalla", "andrés cazalla", "andres cazalla" };

        public override bool PuedeParsar(string texto) =>
            Identificadores.Any(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"FACTURA\s+(20[\d]{5})\s+",
            RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"FACTURA\s+20[\d]{5}\s+([\d/]+)\s",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"Calle Newton, 20 (.*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"\b([A-Z]?\d{7,8}[A-Z]?)\s+\d*\s*semirueda",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"([\d,.]+)\s[\d,.]+\s.*\n([\d,]+)\s([\d,]+)\s[\d,]+\s([\d,.]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
            };

            factura.Emisor.NIF = "26236236K";
            factura.Emisor.Nombre = "Andrés Cazalla Medina";
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(RegexFecha, texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre, texto, 1);
            factura.Receptor.NIF = ExtraerGrupo(RegexNif, texto, 1).ToUpper();
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.PorcentajeIRPF = ExtraerDecimal(RegexImportes, texto, 3);
            factura.Total = ExtraerDecimal(RegexImportes, texto, 4);
            factura.Estado = DeterminarEstado(factura);

            return factura;
        }
    }
}

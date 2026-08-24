using FacturasApp.Core.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class LidlParser : BaseParser
    {
        public override string Nombre => "LIDL SUPERMERCADOS S.A.U.";
        public override string Nif => "A60195278";

        protected override string[] Identificadores =>
            ["lidl supermercados", "factura"];

        private static readonly Regex RegexNumero = new(
            @"Nº Factura:\s([\d]+)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"\b(.*)\b\sFecha\sTique",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _regexNif = new(
            @"Barcelona[\r\n]+([A-Z]?\d{7,8}[A-Z]?)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected override Regex RegexNif => _regexNif;

        protected override Regex RegexFecha { get; } = new(
            @"\bFecha\sFactura:\s+(.*?)\s",
            RegexOptions.Compiled);

        private static readonly Regex RegexLineaIva = new(
            @"Tipo\sIVA\s(\d{1,2},00)\s([\d.,]+)\s[\d., ]+\s([\d.,]+)",
            RegexOptions.Compiled);

        // ── Parsear devuelve solo la primera línea de IVA (compatibilidad) ──
        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            return ParsearMultiple(texto, rutaArchivo, viaOcr).First();
        }

        public override List<Factura> ParsearMultiple(
            string texto, string rutaArchivo, bool viaOcr)
        {
            var facturas = new List<Factura>();
            var lineasIva = RegexLineaIva.Matches(texto);

            // Datos de cabecera comunes a todas las subfacturas
            string emisorNIF = Nif;
            string emisorNombre = Nombre;
            string numeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            DateTime? fecha = ExtraerFecha(RegexFecha, texto);
            string receptorNombre = ExtraerGrupo(RegexNombre, texto, 1);
            string receptorNIF = ExtraerNif(RegexNif, texto, Nif);

            // Una factura por cada línea de IVA encontrada

            foreach (Match linea in lineasIva)
            {
                var factura = new Factura
                {
                    RutaArchivo = rutaArchivo,
                    ExtractedByOcr = viaOcr,
                    NumeroFactura = numeroFactura,
                    Fecha = fecha,
                    Emisor = new Proveedor
                    { Nombre = emisorNombre, NIF = emisorNIF },
                    Receptor = new Cliente
                    { Nombre = receptorNombre, NIF = receptorNIF },
                    BaseImponible = ParsearDecimal(linea.Groups[2].Value),
                    PorcentajeIVA = ParsearDecimal(linea.Groups[1].Value),
                    TotalFactura = ParsearDecimal(linea.Groups[3].Value)
                };
                factura.Estado = FacturaEstado.Determinar(factura);
                facturas.Add(factura);
            };
            return facturas;
        }
    }
}
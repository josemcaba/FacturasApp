using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class LidlParser : BaseParser
    {
        public override string Nombre => "LIDL SUPERMERCADOS S.A.U.";
        public override string Nif => "A60195278";

        private static readonly string[] Identificadores =
            { "lidl supermercados", "factura"};

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"Nº Factura:\s([\d]+)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"Fecha\sFactura:\s\b(\d{1,2})[/\-\.]((?:\d{1,2})|\S{3})[/\-\.](\d{4})\b",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"\b(.*)\b\sFecha\sTique",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"Barcelona[\r\n]+([A-Z]?\d{7,8}[A-Z]?)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            string receptorNIF = ExtraerGrupo(RegexNif, texto, 1).ToUpper();

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
                    Total = ParsearDecimal(linea.Groups[3].Value)
                };
                factura.Estado = DeterminarEstado(factura);
                facturas.Add(factura);
            };
            return facturas;
        }
    }
}
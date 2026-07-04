using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class MERCADONA: BaseParser
    {
        public override string Nombre => "MERCADONA, S.A.";
        public override string Nif => "A46103834";

        protected override string[] Identificadores =>
            ["mercadona s.a.", "a-46103834"];

        private static readonly Regex RegexNumero = new(
            @"N.\s*Factura:\s*(.*?)\s+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"Razón Social: (.*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _regexNif = new(
            @"NIF: ([A-Z]?\d{7,8}[A-Z]?)\s",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected override Regex RegexNif => _regexNif;

        protected override Regex RegexFecha { get; } = new(
            @"\bFecha\sFactura:\s*(.*?)[\s\n\r]+",
            RegexOptions.Compiled);

        private static readonly Regex RegexLineaIva = new(
            @"(\d+)% ([\d,]+) ([\d,.]+) ([\d,.]+)\r",
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
                    Total = ParsearDecimal(linea.Groups[4].Value)
                };
                factura.Estado = FacturaEstado.Determinar(factura);
                facturas.Add(factura);
            };
            return facturas;
        }
    }
}
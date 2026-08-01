using FacturasApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public partial class DisgoParser : BaseParser
    {
        public override string Nombre => "DISGO, S.A.";
        public override string Nif => "A08806994";

        protected override string[] Identificadores =>
            ["disgo"];

        [GeneratedRegex(@"Fecha[\r\n\s]+(\S+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNumero();

        protected override Regex RegexFecha { get; } = new(
            @"\[P1_Z5]:\s.+[\n\r\s]+(.+)",
            RegexOptions.Compiled);

        [GeneratedRegex(@"\[P1_Z1]:\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNombre();

        private static readonly Regex RegexLineaIva = new(
            @"[\n\r]+([\d.,]+)\s+([\d.,]+)\s+([\d.,]+)",
            RegexOptions.Compiled | RegexOptions.Singleline);

        [GeneratedRegex(@"TOTAL FACTURA.*[\n\r\s]*(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexTotalFactura();

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
            decimal totalFactura = ExtraerDecimal(RegexTotalFactura(), texto, 1);
            decimal subtotales = 0;

            // Eliminamos las lineas duplicadas y las de base imponible 0,00
            var lineasIvaValidas = lineasIva
                .Cast<Match>()
                .Where(m =>
                {
                    string valor = m.Groups[2].Value.Replace(".", "").Replace(",", ".");
                    return decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal baseImp)
                           && baseImp != 0m;
                })
                .DistinctBy(m => (
                    m.Groups[1].Value,
                    m.Groups[2].Value,
                    m.Groups[3].Value
                ));

            // Una factura por cada línea de IVA encontrada
            foreach (Match linea in lineasIvaValidas)
            {
                var factura = CrearFacturaBase(rutaArchivo, viaOcr);
                {
                    factura.NumeroFactura = ExtraerGrupo(RegexNumero(), texto, 1);
                    factura.Fecha = ExtraerFecha(texto);
                    factura.Receptor.Nombre = ExtraerGrupo(RegexNombre(), texto, 1);
                    factura.Receptor.NIF = ExtraerNif(texto);
                    factura.BaseImponible = ParsearDecimal(linea.Groups[1].Value);
                    factura.PorcentajeIVA = ParsearDecimal(linea.Groups[2].Value);
                    factura.CuotaIVA = ParsearDecimal(linea.Groups[3].Value);
                    factura.TotalFactura = factura.BaseImponible + factura.CuotaIVA;
                    factura.Estado = FacturaEstado.Determinar(factura);
                }
                subtotales += factura.TotalFactura;
                facturas.Add(factura);
            }
            if (subtotales != totalFactura)
            {
                // Si la suma de los totales de las facturas no coincide con el total de la factura, marcar como error
                foreach (var factura in facturas)
                {
                    factura.MensajeError.Add($"La suma de los sub-totales ({subtotales}) no coincide con el total de la factura ({totalFactura}).");
                    factura.Estado = EstadoFactura.Error;
                }
            }
            return facturas;
        }
    }
}
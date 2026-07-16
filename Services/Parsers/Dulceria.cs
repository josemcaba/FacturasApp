using FacturasApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public partial class DulceriaParser : BaseParser
    {
        public override string Nombre => "Dulcería 17, S.L.";
        public override string Nif => "B11425964";

        protected override string[] Identificadores =>
            ["40.0015231", "40.080372"];

        [GeneratedRegex(@"Número[\n\r]+(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNumero();

        [GeneratedRegex(@"\[P1_Z1]:\s*(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNombre();

        // Crear método GeneratedRegex específico
        [GeneratedRegex(@"([\d]{2}\.[\d]{3}\.[\d]{3}-.)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNifEspecifico();
        protected override Regex RegexNif { get; } = RegexNifEspecifico();

        private static readonly Regex RegexLineaIva = new(
            @"([-\d.,]+)\s+([\d.,]+)\s*%\s+([-\d.,]+)",
            RegexOptions.Compiled | RegexOptions.Singleline);

        [GeneratedRegex(@"TOTAL EURO.*[\n\r]*([-\d.,]+)", RegexOptions.Compiled)]
        private static partial Regex RegexTotal();

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
            decimal totalFactura = ExtraerDecimal(RegexTotal(), texto, 1);
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
                    factura.Total = factura.BaseImponible + factura.CuotaIVA;
                    factura.Estado = FacturaEstado.Determinar(factura);
                }
                subtotales += factura.Total;
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
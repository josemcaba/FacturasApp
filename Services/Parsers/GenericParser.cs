using System.Text.RegularExpressions;
using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public partial class GenericParser : BaseParser
    {
        public override string Nombre => "Parser Genérico";
        public override string Nif => "General";
        public override bool PuedeParsar(string texto) => true;

        [GeneratedRegex(@"(?:factura|fra\.?|nº|n[uú]mero)[:\s#]*([A-Z0-9][-A-Z0-9/\\]{2,20})", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNumero();

        [GeneratedRegex(@"\b([A-Z]?\d{7,8}[A-Z])\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNif();

        [GeneratedRegex(@"(?:base\s+imponible|subtotal|base)[:\s]*([\d.,]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexBase();

        [GeneratedRegex(@"IVA\s*(\d{1,2})\s*%", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexIva();

        [GeneratedRegex(@"(?:total\s+factura|total\s+a\s+pagar|importe\s+total|total)[:\s]*([\d.,]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexTotal();

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var nifs = ExtraerTodosLosNifs(texto);

            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
                NumeroFactura = ExtraerGrupo(RegexNumero(), texto, 1),
                Fecha = ExtraerFecha(RegexFecha, texto),
                Emisor = new Proveedor
                {
                    Nombre = this.Nombre,
                    NIF = nifs.Count > 0 ? nifs[0] : string.Empty
                },
                Receptor = new Cliente
                {
                    NIF = nifs.Count > 1 ? nifs[1] : string.Empty
                },
                BaseImponible = ExtraerDecimal(RegexBase(), texto, 1),
                Total = ExtraerDecimal(RegexTotal(), texto, 1)
            };

            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }

        private static List<string> ExtraerTodosLosNifs(string texto)
        {
            return [.. RegexNif().Matches(texto)
                .Select(m => m.Groups[1].Value.ToUpper())
                .Distinct()];
        }
    }
}
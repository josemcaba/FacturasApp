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

        [GeneratedRegex(@"(?:base\s+imponible|subtotal|base)[:\s]*([\d.,]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexBase();

        [GeneratedRegex(@"IVA\s*(\d{1,2})\s*%", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexIva();

        [GeneratedRegex(@"(?:total\s+factura|total\s+a\s+pagar|importe\s+total|total)[:\s]*([\d.,]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexTotal();

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
                NumeroFactura = ExtraerGrupo(RegexNumero(), texto, 1),
                Fecha = ExtraerFecha(RegexFecha, texto),
                Emisor = new Proveedor
                {
                    Nombre = this.Nombre,
                    NIF = ExtraerNif(texto)
                },
                Receptor = new Cliente
                {
                    NIF = ExtraerNif(texto)
                },
                BaseImponible = ExtraerDecimal(RegexBase(), texto, 1),
                TotalFactura = ExtraerDecimal(RegexTotal(), texto, 1)
            };

            return factura;
        }
    }
}
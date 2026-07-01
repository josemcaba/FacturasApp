using FacturasApp.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public partial class OscarAriasParser : BaseParser
    {
        public override PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;
        public override string Nombre => "Oscar Arias Merino";
        public override string Nif => "74890980J";

        private static readonly string[] Identificadores =
            ["oscar arias merino", "74890980-j", "deudor"];

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        [GeneratedRegex(@"(FV[\d]{5})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNumero();

        [GeneratedRegex(@"CLIENTE[\r\n]+(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNombre();

        // Base Imponible
        [GeneratedRegex(@"IMPONIBLE\s+([,\.0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexBaseImponible();

        // Descuento
        [GeneratedRegex(@"DESCUENTO\s+([-,\.0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexDescuento();

        // Total Parcial en caso de que no haya Base Imponible (compatibilidad)
        [GeneratedRegex(@"TOTAL\s+PARCIAL\s+([,\.0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexTotalParcial();

        // Porcentaje
        [GeneratedRegex(@"\(([,\.0-9]+)%\)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexPorcentaje();

        // Total factura
        [GeneratedRegex(@"TOTAL\s+([,\.0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexTotalFactura();

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            texto = EliminarDuplicadosNoNumericos(texto);
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
            };

            factura.Emisor.NIF = Nif;
            factura.Emisor.Nombre = Nombre;
            factura.NumeroFactura = ExtraerGrupo(RegexNumero(), texto, 1);
            factura.Fecha = ExtraerFecha(texto);  // ← Usa el genérico de BaseParser
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre(), texto, 1);
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexBaseImponible(), texto, 1);
            if (factura.BaseImponible == 0)
                factura.BaseImponible = ExtraerDecimal(RegexTotalParcial(), texto, 1);
            factura.BaseImponible += ExtraerDecimal(RegexDescuento(), texto, 1);
            factura.PorcentajeIVA = decimal.Parse(EliminarDuplicadosNumericos(ExtraerGrupo(RegexPorcentaje(), texto, 1)));
            if (factura.PorcentajeIVA == 1.4m)
            {
                factura.PorcentajeIVA = 10m;
                factura.PorcentajeRE = 1.4m;
            }
            factura.Total = ExtraerDecimal(RegexTotalFactura(), texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }

        public static string EliminarDuplicadosNoNumericos(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            StringBuilder resultado = new();
            char? ultimoCaracter = null;

            foreach (char c in texto)
            {
                bool esNumero = c >= '0' && c <= '9';

                if (esNumero)
                {
                    // Los números siempre se agregan, sin importar si se repiten
                    resultado.Append(c);
                    ultimoCaracter = c;
                }
                else
                {
                    // No es número: solo se agrega si es diferente al anterior
                    if (!ultimoCaracter.HasValue || c != ultimoCaracter.Value)
                    {
                        resultado.Append(c);
                        ultimoCaracter = c;
                    }
                }
            }

            return resultado.ToString();
        }

        public static string EliminarDuplicadosNumericos(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            StringBuilder resultado = new();
            char? ultimoCaracter = null;

            foreach (char c in texto)
            {
                bool esNumero = c >= '0' && c <= '9';

                if (!esNumero)
                {
                    // Los no números siempre se agregan, sin importar si se repiten
                    resultado.Append(c);
                    ultimoCaracter = c;
                }
                else
                {
                    // Si es número: solo se agrega si es diferente al anterior
                    if (!ultimoCaracter.HasValue || c != ultimoCaracter.Value)
                    {
                        resultado.Append(c);
                        ultimoCaracter = c;
                    }
                }
            }

            return resultado.ToString();
        }
    }
}
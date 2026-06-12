using FacturasApp.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public partial class DulceriaParser : BaseParser
    {
        public override PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;
        public override string Nombre => "Dulcería 17, S.L.";
        public override string Nif => "B11425964";

        private static readonly string[] Identificadores =
            ["40.0015231", "40.080372"];

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        [GeneratedRegex(@"(1-[\d]{7})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNumero();

        [GeneratedRegex(@"(.+)[\r\n]+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNombre();

        // Crear método GeneratedRegex específico
        [GeneratedRegex(@"([\d]{2}\.[\d]{3}\.[\d]{3}-.)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNifEspecifico();

        // Sobrescribir usando el método generado
        protected override Regex RegexNif { get; } = RegexNifEspecifico();

        // Importes
        [GeneratedRegex(@"([,\.0-9]+)\s+([0124]+)%\s[,\.0-9]+[ \r\n]+([,\.0-9]+)[ \r\n]+Nº\.R\.S\.", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexImportes();

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
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
            factura.BaseImponible = ExtraerDecimal(RegexImportes(), texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes(), texto, 2);
            factura.Total = ExtraerDecimal(RegexImportes(), texto, 3);
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
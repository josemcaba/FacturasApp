using FacturasApp.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public partial class FiestaParser : BaseParser
    {
        public override PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.OrdenadoPosicion;
        public override string Nombre => "Fiesta Colombina, S.L.U.";
        public override string Nif => "B85905412";

        private static readonly string[] Identificadores =
            ["fiesta colombina s.l.u", "n.i.f.:b85905412"];

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        [GeneratedRegex(@"FACTURA:\s+(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNumero();

        protected override Regex RegexFecha { get; } = new(
            @"\bFECHA\s+(\d{1,2}[\/\.-](?:\d{1,2}|\D{3})[\/\.-]\d{2,4})\s+FECHA\b",
            RegexOptions.Compiled);

        [GeneratedRegex(@"NOMBRE:\s+(.+?\s+.+?\s+.+?)\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNombre();

        // Base Imponible
        [GeneratedRegex(@"BASE IMPONIBLE EUR 10% ([,\.0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexBaseImponible();

        // TOTAL FACTURA
        [GeneratedRegex(@"\bTOTAL EUR ([,\.0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexTotalFactura();

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
            factura.BaseImponible = ExtraerDecimal(RegexBaseImponible(), texto, 1);
            factura.PorcentajeIVA = 10;
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
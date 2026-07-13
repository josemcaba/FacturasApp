using FacturasApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class PetroprixParser : BaseParser
    {
        public override string Nombre => "PETROPRIX ENERGIA S.L.";
        public override string Nif => "B23709892";
        public override string Concepto => "624";

        protected override string[] Identificadores =>
            ["B23709892", "PETROPRIX"];

        protected override Regex RegexFecha { get; } = new(
            @"Fecha:\s*(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexNumero = new(
            @"FACTURA\s+(\d+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"CLIENTE[\n\r]+(.+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex RegexReceptorNif = new(
            @"CIF/NF\s*:\s*(\d{8}[A-Z])",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"Factura[\r\n]*([\d.,]+).*?([\d.,]+)\s*.*?([\d.,]+)\s+.*?([\d.,]+)",
            RegexOptions.Compiled);

        private DateTime? ExtraerFechaEspañol(string texto)
        {
            var m = RegexFecha.Match(texto);
            if (!m.Success) return null;

            string fechaStr = m.Groups[1].Value.Trim();
            string[] formatos = ["d 'de' MMMM 'de' yyyy", "d 'de' MMM 'de' yyyy"];

            return DateTime.TryParseExact(
                fechaStr, formatos,
                new CultureInfo("es-ES"),
                DateTimeStyles.None,
                out var fecha) ? fecha : null;
        }

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFechaEspañol(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerGrupo(RegexReceptorNif, texto, 1);

            var mImportes = RegexImportes.Match(texto);
            if (mImportes.Success)
            {
                factura.BaseImponible = ParsearDecimal(mImportes.Groups[1].Value);
                factura.PorcentajeIVA = ParsearDecimal(mImportes.Groups[2].Value);
                factura.CuotaIVA = ParsearDecimal(mImportes.Groups[3].Value);
                factura.Total = ParsearDecimal(mImportes.Groups[4].Value);
            }

            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

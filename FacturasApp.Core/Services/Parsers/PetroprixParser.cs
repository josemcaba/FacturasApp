using FacturasApp.Core.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class PetroprixParser : BaseParser
    {
        public override string Nombre => "PETROPRIX ENERGIA S.L.";
        public override string Nif => "B23709892";

        protected override string[] Identificadores =>
            ["B23709892", "PETROPRIX"];

        protected override Regex RegexFecha { get; } = new(
            @"Fecha:\s*(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexNumero = new(
            @"FACTURA\s+(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"Nombre:\s+(.+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex RegexImportes = new(
            @"IVA[\r\n]+([\d.,]+).*?([\d.,]+)\s*%.*?([\d.,]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"Total Factura[\r\n]+([\d.,]+)",
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

            factura.ConceptoGasto = "624";
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFechaEspañol(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

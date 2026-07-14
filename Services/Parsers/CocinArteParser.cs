using DocumentFormat.OpenXml.Presentation;
using FacturasApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class CocinArteParser: BaseParser
    {
        public override string Nombre => "PEDRO RODRIGUEZ GONZALEZ";
        public override string Nif => "33367623M";  

        protected override string[] Identificadores =>
            ["33.367.623-M", "cocinarte"];

        private static readonly Regex RegexNumero = new(
            @"FACTURA N.\s*(\S+)",
            RegexOptions.Compiled);
        
        private static readonly Regex RegexReceptorNombre = new(
            @"Cliente:\s+(.+)",
            RegexOptions.Compiled);
        protected override Regex RegexNif { get; } = new(
            @"\b(\d{2}\.\d{3}\.\d{3}-[A-Z])\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexBase = new(
            @"BASE IMPONIBLE:\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexIva = new(
            @"IVA \(([\d,.]+)%\):\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"TOTAL FACTURA:\s+([\d,.]+)",
            RegexOptions.Compiled);

        protected override Regex RegexFecha { get; } = new(
            @"Málaga,\s+(.+)",
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
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexBase, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexIva, texto, 1);
            factura.CuotaIVA = ExtraerDecimal(RegexIva, texto, 2);
            factura.Total = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

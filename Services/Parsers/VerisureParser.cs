using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class VerisureParser : BaseParser
    {
        public override string Nombre => "SECURITAS DIRECT ESPAÑA S.A.U.";
        public override string Nif => "A26106013";

        protected override string[] Identificadores =>
            ["A26106013", "securitas", "direct", "españa"];

        private static readonly Regex RegexNumero = new(
            @"Nº de factura:\s*(\S+)",
            RegexOptions.Compiled);

        protected override Regex RegexFecha { get; } = new(
            @"Fecha de factura:\s*(.*)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"Titular:\s*(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexBaseImponible = new(
            @"Subtotal\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexIva = new(
            @"IVA\s+([\d]+)\s*%\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"Total factura\s+([\d,.]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexBaseImponible, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexIva, texto, 1);
            factura.CuotaIVA = ExtraerDecimal(RegexIva, texto, 2);
            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

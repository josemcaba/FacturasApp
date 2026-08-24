using FacturasApp.Core.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class SewanParser : BaseParser
    {
        public override string Nombre => "SEWAN ESPAÑA S.L.U.";
        public override string Nif => "B73619215";

        protected override string[] Identificadores =>
            ["B73619215", "SEWAN"];

        protected override Regex RegexFecha { get; } = new(
            @"Fecha:\s*(\d{2})[\/\-](\d{2})[\/\-](\d{4})",
            RegexOptions.Compiled);

        private static readonly Regex RegexNumero = new(
            @"Factura n°:\s*(\d+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"B73619215[\n\r]+(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexBaseImponible = new(
            @"Total factura impuestos no incluidos\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexPorcentajeIva = new(
            @"Total IVA\s+([\d,.]+)%",
            RegexOptions.Compiled);

        private static readonly Regex RegexCuotaIva = new(
            @"Total IVA\s+[\d,.]+%\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"Total factura impuestos incluidos\s+([\d,.]+)\s*€",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexBaseImponible, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexPorcentajeIva, texto, 1);
            factura.CuotaIVA = ExtraerDecimal(RegexCuotaIva, texto, 1);
            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

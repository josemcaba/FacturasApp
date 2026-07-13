using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class EurocabosParser : BaseParser
    {
        public override string Nombre => "Eurocabos Sur S.L.";
        public override string Nif => "B92188812";

        protected override string[] Identificadores =>
            ["B92188812", "Eurocabos Sur"];

        protected override Regex RegexFecha { get; } = new(
            @"FECHA:\s*(\d{2})[\/\-](\d{2})[\/\-](\d{4})",
            RegexOptions.Compiled);

        private static readonly Regex RegexNumero = new(
            @"NÚMERO:\s*(\S+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"Dirección:\s*\n(.+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex RegexImportes = new(
            @"Importe neto.*\n([\d,.]+)\s+([\d,.]+)\s+([\d,.]+)\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"TOTAL\s+\(EUR\):\s+([\d,.]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);

            var mImportes = RegexImportes.Match(texto);
            if (mImportes.Success)
            {
                factura.BaseImponible = ParsearDecimal(mImportes.Groups[1].Value);
                factura.PorcentajeIVA = ParsearDecimal(mImportes.Groups[3].Value);
                factura.CuotaIVA = ParsearDecimal(mImportes.Groups[4].Value);
            }

            factura.Total = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

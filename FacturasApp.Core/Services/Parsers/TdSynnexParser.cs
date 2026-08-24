using DocumentFormat.OpenXml.Presentation;
using FacturasApp.Core.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class TdSynnexParser: BaseParser
    {
        public override string Nombre => "TD SYNNEX SPAIN, S.L.U.";
        public override string Nif => "B58728585";  

        protected override string[] Identificadores =>
            ["ESB58728585", "td", "synnex"];

        private static readonly Regex RegexNumero = new(
            @"N. de factura[\n\r]+\d+\s+[0-9.]+\s+(\d+)",
            RegexOptions.Compiled);
        
        private static readonly Regex RegexReceptorNombre = new(
            @"Empresa[\n\r]+(.*)",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"Importe IVA\s+([\d,.]+)\s*%\s+([\d,.]+).+?([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"Total a pagar\s+([\d,.]+)",
            RegexOptions.Compiled);

        protected override Regex RegexFecha { get; } = new(
            @"N. de factura[\n\r]+\d+\s+([0-9.]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 2);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 1);
            factura.CuotaIVA = ExtraerDecimal(RegexImportes, texto, 3);
            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

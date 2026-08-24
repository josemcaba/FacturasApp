using FacturasApp.Core.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class JuanLucasParser: BaseParser
    {
        public override string Nombre => "ACTIVA JUAN LUCAS S.L.";
        public override string Nif => "B93666816";

        protected override string[] Identificadores =>
            ["B93666816", "juan", "lucas", "www.juanlucas.com"];

        private static readonly Regex RegexNumero = new(
            @"FACTURA[\n\r]+(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"\[P1_Z1]:\s+(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"BASE % IVA[\n\r]+([\d,.]+)\s+([\d,.]+)\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"TOTAL FRA[\n\r]+([\d,.]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.CuotaIVA = ExtraerDecimal(RegexImportes, texto, 3);
            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

using DocumentFormat.OpenXml.Presentation;
using FacturasApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class RyanairDacParser: BaseParser
    {
        public override string Nombre => "RYANAIR DAC";
        public override string Nif => "W0071513F";  

        protected override string[] Identificadores =>
            ["ESW0071513F", "ryanair", "spain"];

        private static readonly Regex RegexNumero = new(
            @"Factura n.:\s+(.*)",
            RegexOptions.Compiled);
        
        private static readonly Regex RegexReceptorNombre = new(
            @"Tipo de pago:.+[\n\r]+(.*)",
            RegexOptions.Compiled);
        protected override Regex RegexFecha { get; } = new(
            @"Fecha de emisión:\s*([\d\/]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"Tarifa\s+([\d.,]+)\s+([\d.,]+)%\s+([\d.,]+)\s+([\d.,]+)",
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
            factura.Total = ExtraerDecimal(RegexImportes, texto, 4);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

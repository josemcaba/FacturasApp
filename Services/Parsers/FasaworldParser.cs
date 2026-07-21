using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    [Obsolete("Usar emisores.xml en su lugar")]
    public class FasaworldParser: BaseParser
    {
        public override string Nombre => "FASAWORLD S.L.";
        public override string Nif => "B64713662";  

        protected override string[] Identificadores =>
            ["B64713662", "fasaworld.com"];

        private static readonly Regex RegexNumero = new(
            @"Fecha[\n\r]+(\S+)\s+(\S+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"Cliente:[\n\r]+(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"FACTURA[\n\r]+([\d,.]+)\s+([\d,.]+)\s+([\d,.]+)[\n\r]+([\d,.]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1) + "/" + ExtraerGrupo(RegexNumero, texto, 2);
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

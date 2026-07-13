using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class InstantByteParser : BaseParser
    {
        public override string Nombre => "INSTANT BYTE, S.L.";
        public override string Nif => "B83680082";

        protected override string[] Identificadores =>
            ["B83680082", "instantbyte"];


        private static readonly Regex RegexNumero = new(
            @"Factura:\s+(\S+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"Cliente Nº:.*[\r\n]+(.+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex BaseImponible = new(
            @"([\d,.]+)[\r\n]+Base\s+Imponible",
            RegexOptions.Compiled);

        private static readonly Regex RegexIva = new(
            @"IVA\s+\((\d+)%\)\s+([\d,.]+)",
                RegexOptions.Compiled);

        private static readonly Regex Total = new(
            @"Importe\s+Total.*?([\d,.]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1);
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ParsearDecimal(ExtraerGrupo(BaseImponible, texto, 1));

            var mIva = RegexIva.Match(texto);
            if (mIva.Success) {
                factura.PorcentajeIVA = ParsearDecimal(mIva.Groups[1].Value);
                factura.CuotaIVA = ParsearDecimal(mIva.Groups[2].Value);
            }
                
            factura.Total = ParsearDecimal(ExtraerGrupo(Total, texto, 1));

            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

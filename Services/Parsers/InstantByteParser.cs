using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class InstantByteParser : BaseParser
    {
        public override string Nombre => "INSTANT BYTE, S.L.";
        public override string Nif => "B83680082";

        protected override string[] Identificadores =>
            ["palacios", "instantbyte.com"];


        private static readonly Regex RegexNumero = new(
            @"Factura:\s+(\S+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"Cliente .*[\r\n]+(.+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex RegexImportes = new(
            @"([\d,.]+)[\r\n]+([\d,.]+)[\r\n]+0,00[\r\n]+([\d,.]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1);
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = 21m;
            factura.CuotaIVA = ExtraerDecimal(RegexImportes, texto, 2);
            factura.TotalFactura = ExtraerDecimal(RegexImportes, texto, 3);

            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

using FacturasApp.Core.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class ComunicacionesCloudParser : BaseParser
    {
        public override string Nombre => "COMUNICACIONES Y SOLUCIONES CLOUD, S.L.";
        public override string Nif => "B56255862";

        protected override string[] Identificadores =>
            ["B56255862", "CLOUD", "iberikatelecom"];


        private static readonly Regex RegexNumero = new(
            @"FACTURA\s+.*\n*(\S+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"Cliente:\s*\n(.+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex RegexImportes = new(
            @"EUROS[\r\n]*([\d,.]+).*?([\d,.]+).*?([\d,.]+).*?[\r\n]*([\d,.]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim().Trim('.');
            factura.Receptor.NIF = ExtraerNif(texto);

            var mImportes = RegexImportes.Match(texto);
            if (mImportes.Success) {
                factura.BaseImponible =ParsearDecimal(mImportes.Groups[1].Value);
                factura.PorcentajeIVA = ParsearDecimal(mImportes.Groups[2].Value);
                factura.CuotaIVA = ParsearDecimal(mImportes.Groups[3].Value);
                factura.TotalFactura = ParsearDecimal(mImportes.Groups[4].Value);
            }

            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

using FacturasApp.Core.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Core.Services.Parsers
{
    public class HostaliaParser : BaseParser
    {
        public override string Nombre => "TELEFÓNICA SOLUCIONES I y C ESPAÑA SAU";
        public override string Nif => "A78053147";

        protected override string[] Identificadores =>
            ["A78053147", "hostalia"];

        private static readonly Regex RegexNumero = new(
            @"Nº Factura:\s*(\S+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"DATOS FISCALES.*\n(.+)",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex RegexImportes = new(
            @"Importe Neto\s+([\d,.]+)\s*.*\nIVA\s+(\d+)\s*%\s+([\d,.]+)\s*.*\nTOTAL FACTURA\s+([\d,.]+)",
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
                factura.PorcentajeIVA = ParsearDecimal(mImportes.Groups[2].Value);
                factura.CuotaIVA = ParsearDecimal(mImportes.Groups[3].Value);
                factura.TotalFactura = ParsearDecimal(mImportes.Groups[4].Value);
            }

            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

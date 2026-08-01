using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class CathedralSwParser: BaseParser
    {
        public override string Nombre => "CATHEDRAL SOFTWARE S.L.";
        public override string Nif => "B16505364";

        protected override string[] Identificadores =>
            ["B16505364", "cathedral", "binissalem"];

        private static readonly Regex RegexNumero = new(
            @"Nº FACTURA[\n\r]+(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"\[P1_Z1]:\s+(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"bruto\s+([\d,.]+)[\n\r]+([\d,.]+)%\s+([\d,.]+)[\n\r]+([\d,.]+)",
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
            factura.TotalFactura = ExtraerDecimal(RegexImportes, texto, 4);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

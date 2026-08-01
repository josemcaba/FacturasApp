using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class EuroDepotParser: BaseParser
    {
        public override string Nombre => "EURO DEPOT ESPAÑA S.A.U.";
        public override string Nif => "A62018064";

        protected override string[] Identificadores =>
            ["brico", "depot"];

        private static readonly Regex RegexNumero = new(
            @"N.\s+factura\s+FT\s+(\S+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"Dirección de factura[\r\n]+(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"Total\s+.*?([\d,.]+)\s+([\d,.]+)\s+[\d,.]+\s+([\d,.]+)",
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

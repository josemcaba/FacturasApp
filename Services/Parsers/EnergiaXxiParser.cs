using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class EnergiaXxiParser: BaseParser
    {
        public override string Nombre => "ENERGIA XXI COMERCIALIZADORA S.L.U.";
        public override string Nif => "B82846825";  

        protected override string[] Identificadores =>
            ["B82846825", "comercializadora"];

        private static readonly Regex RegexNumero = new(
            @"N.factura:\s+(\S+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexReceptorNombre = new(
            @"Titular del contrato:\s+(.+)?NIF",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"FACTURA[\n\r]+([\d,.]+)\s+([\d,.]+)\s+([\d,.]+)[\n\r]+([\d,.]+)",
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

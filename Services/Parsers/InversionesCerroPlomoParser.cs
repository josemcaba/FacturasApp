using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class InversionesCerroPlomoParser : BaseParser
    {
        public override string Nombre => "INVERSIONES CERRO EL PLOMO S.L.";
        public override string Nif => "B93538783";

        protected override string[] Identificadores =>
            ["B93538783", "INVERSIONES CERRO EL PLOMO"];

        private static readonly Regex RegexNumero = new(
            @"^(\d{6})\s+\d{2}/\d{2}/\d{4}",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex RegexCliente = new(
            @"\[Zona2]:\s*(.*)",
            RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"BASE IMPONIBLE.*[\r\n]+([\d,.]+)\s+([\d,.]+)\s+([\d,.]+)\s+([\d,.]+)\s+([\d,.]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"TOTAL\s+FACTURA\s*:\s*([\d,.]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);
            factura.ConceptoGasto = "625";

            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexCliente, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);

            var mImportes = RegexImportes.Match(texto);
            if (mImportes.Success)
            {
                factura.BaseImponible = ParsearDecimal(mImportes.Groups[1].Value);
                factura.PorcentajeIVA = ParsearDecimal(mImportes.Groups[2].Value);
                factura.CuotaIVA = ParsearDecimal(mImportes.Groups[3].Value);
                factura.PorcentajeIRPF = ParsearDecimal(mImportes.Groups[4].Value);
                factura.CuotaIRPF = ParsearDecimal(mImportes.Groups[5].Value);
            }

            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

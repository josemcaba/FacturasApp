using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class IonosParser: BaseParser
    {
        public override string Nombre => "IONOS Cloud S.L.U.";
        public override string Nif => "B85049435";  

        protected override string[] Identificadores =>
            ["85049435", "ionos", "cloud"];

        private static readonly Regex RegexNumero = new(
            @"N.. de factura:\s+(.*)",
            RegexOptions.Compiled);
        
        private static readonly Regex RegexReceptorNombre = new(
            @"(.*)[\n\r]+NIF\/CIF",
            RegexOptions.Compiled);

        protected override Regex RegexFecha { get; } = new(
            @"Fecha de facturación:\s+(.*)",
            RegexOptions.Compiled);

        private static readonly Regex RegexBase = new(
            @"Total \(base imponible\)\s+([\d.,]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexIva = new(
            @"IVA \(([\d.,]+)\s*%\)\s+([\d.,]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"Total a pagar\s+([\d.,]+)",
            RegexOptions.Compiled);

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.ConceptoGasto = "629";
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexReceptorNombre, texto, 1).Trim();
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexBase, texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexIva, texto, 1);
            factura.CuotaIVA = ExtraerDecimal(RegexIva, texto, 2);
            factura.TotalFactura = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}

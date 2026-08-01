using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class PescaderiaMarengoParser : BaseParser
    {
        public override string Nombre => "Pescadería Marengo";
        public override string Nif => "33384986A";

        protected override string[] Identificadores =>
            ["Marengo", "33384986-A"];

        private static readonly Regex RegexNumero = new(
            @"Nº factura\s*(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"Pescadería\s+Marengo\s+(.+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _regexNif = new(
            @"\b33384986-A\s+(.+)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected override Regex RegexNif => _regexNif;

        private static readonly Regex RegexImportes = new(
            @"Base\s+(.+?)\s+IVA\s+.+?\s+Total\s+(.+?)\s+",
            RegexOptions.Compiled);


        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
            };

            factura.Emisor.NIF = Nif;
            factura.Emisor.Nombre = Nombre;
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            string texto_f = Regex.Replace(texto, @",", "");
            factura.Fecha = ExtraerFecha(RegexFecha, texto_f);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre, texto, 1);
            factura.Receptor.NIF = ExtraerNif(RegexNif, texto, Nif);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = 10;
            factura.TotalFactura = ExtraerDecimal(RegexImportes, texto, 2);
            factura.Estado = FacturaEstado.Determinar(factura);
            
            return factura;
        }
    }
}

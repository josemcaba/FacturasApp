using CsvHelper;
using FacturasApp.Models;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class PescaderiaMarengoParser : BaseParser
    {
        public override string Nombre => "Pescadería Marengo";
        public override string Nif => "33384986A";

        private static readonly string[] Identificadores =
            { "Marengo", "33384986-A"};

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"Nº factura\s*(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"Pescadería\s+Marengo\s+(.+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"\b33384986-A\s+(.+)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            factura.Total = ExtraerDecimal(RegexImportes, texto, 2);
            factura.Estado = DeterminarEstado(factura);
            
            return factura;
        }
    }
}

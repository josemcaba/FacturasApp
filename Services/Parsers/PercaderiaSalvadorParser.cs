using CsvHelper;
using FacturasApp.Models;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class PescaderiaSalvadorParser : BaseParser
    {
        public override string Nombre => "Pescadería Salvador";
        public override string Nif => "25041071M";

        private static readonly string[] Identificadores =
            { "Salvador", "25041071-M"};

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"Nº factura\s*(.+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"Pescadería\s+Salvador\s+(.+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"\b25041071-M\s+(.+)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"Base\s+(.+?)\s+IVA\s+.+?\s+Total\s+(.+?)\s+",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotal = new(
            @"\s+IVA\s+.+?\s+Total\s+(.+?)\s+",
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
            factura.NumeroFactura = ExtraerGrupo(RegexNumero, texto,1);
            string texto_f = Regex.Replace(texto, @"(\d{1}),(\d{3}\s)", "$1$2");
            texto_f = Regex.Replace(texto_f, @"(\d{1}),(\d{1})", "$1/$2");
            factura.Fecha = ExtraerFecha(RegexFecha, texto_f);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre, texto, 1);
            factura.Receptor.Nombre = AcortaNombreCliente(factura);
            factura.Receptor.NIF = ExtraerNif(RegexNif, texto, Nif);
            factura.BaseImponible = ExtraerDecimal(RegexImportes, texto, 1);
            factura.PorcentajeIVA = 10;
            factura.Total = ExtraerDecimal(RegexTotal, texto, 1);
            factura.Estado = FacturaEstado.Determinar(factura);
            
            return factura;
        }
        public static string AcortaNombreCliente(Factura factura)
        {
            string nombre = factura.Receptor?.Nombre ?? string.Empty;

            return nombre switch
            {
                var n when n.StartsWith("Ramírez Sánchez S.L.")    => "Ramírez Sánchez S.L. 'Rest Refrectorium'",
                var n when n.StartsWith("Luis Gaspar Rodríguez")   => "Luis Gaspar Rodríguez 'Rest. El Rengue'",
                var n when n.StartsWith("Miguel Ángel Vigo Gómez") => "Miguel A. Vigo 'Marisquería La Marisma'",
                _ => nombre
            };
        }
    }
}

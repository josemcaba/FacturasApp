using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public class EmasaParser : BaseParser
    {
        public override string Nombre => "Empresa Municipal Aguas de Málaga S.A.";
        public override string Nif => "A29185519";
        public override string Concepto => "628"; // Suministro de agua 628 (G15)

        private static readonly string[] Identificadores =
            { "emasa", "plaza general torrijos", "agua"};

        public override bool PuedeParsar(string texto) =>
            Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        private static readonly Regex RegexNumero = new(
            @"FACTURA:\s*(\S+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexFecha = new(
            @"EMISIÓN:\s*\b(\d{1,2})[/\-\.](\d{1,2})[/\-\.](\d{4})\b",
            RegexOptions.Compiled);

        private static readonly Regex RegexNombre = new(
            @"www.emasa.es\r*\n*(.+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexNif = new(
            @"NIF/CIF:\s*(\S+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RegexImportes = new(
            @"\(Ley 9/2010\) ([\d,]+) €\r\n+IVA \(B.I. ([\d,]+)\) (\d+)% ([\d,]+)",
            RegexOptions.Compiled);

        private static readonly Regex RegexTotalFactura = new(
            @"TOTAL FACTURA ([\d,]+) €",
            RegexOptions.Compiled);

        // ── Parsear devuelve solo la primera línea de IVA (compatibilidad) ──
        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            return ParsearMultiple(texto, rutaArchivo, viaOcr).First();
        }
        public override List<Factura> ParsearMultiple
            (string texto, string rutaArchivo, bool viaOcr)
        {
            var facturas = new List<Factura>();

            // Datos de cabecera comunes a todas las subfacturas
            string emisorNIF = Nif;
            string emisorNombre = Nombre;
            string conceptoFactura = Concepto;
            string numeroFactura = ExtraerGrupo(RegexNumero, texto, 1);
            DateTime? fecha = ExtraerFecha(RegexFecha, texto);
            string receptorNombre = ExtraerGrupo(RegexNombre, texto, 1);
            string receptorNIF = ExtraerGrupo(RegexNif, texto, 1).ToUpper();


            var parteEmasa= new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
                NumeroFactura = numeroFactura,
                Fecha = fecha,
                Concepto = conceptoFactura,
                Emisor = new Proveedor
                { Nombre = emisorNombre, NIF = emisorNIF },
                Receptor = new Cliente
                { Nombre = receptorNombre, NIF = receptorNIF },
                BaseImponible = ExtraerDecimal(RegexImportes, texto, 2),
                PorcentajeIVA = ExtraerDecimal(RegexImportes, texto, 3),
                Total = ExtraerDecimal(RegexImportes, texto, 4) + ExtraerDecimal(RegexImportes, texto, 2)
            };
            parteEmasa.Estado = DeterminarEstado(parteEmasa);
            facturas.Add(parteEmasa);

            var parteJunta = new Factura
            {
                RutaArchivo = rutaArchivo,
                ExtractedByOcr = viaOcr,
                NumeroFactura = numeroFactura,
                Fecha = fecha,
                Concepto = conceptoFactura,
                Emisor = new Proveedor
                { Nombre = emisorNombre, NIF = emisorNIF },
                Receptor = new Cliente
                { Nombre = receptorNombre, NIF = receptorNIF },
                BaseImponible = ExtraerDecimal(RegexImportes, texto, 1),
                Total = ExtraerDecimal(RegexImportes, texto, 1)
            };
            parteJunta.Estado = DeterminarEstado(parteJunta);
            facturas.Add(parteJunta);

            var totalFactura = ExtraerDecimal(RegexTotalFactura, texto, 1);
            if (parteEmasa.Total + parteJunta.Total != totalFactura)
            {   parteEmasa.ErrorMensaje = $"La suma de partes ({parteEmasa.Total + parteJunta.Total} €) no coincide con el total de factura ({totalFactura} €)";
                parteEmasa.Estado = EstadoFactura.Error;
                parteJunta.ErrorMensaje = parteEmasa.ErrorMensaje;
                parteJunta.Estado = EstadoFactura.Error;
            }

            return facturas;
        }
    }
}

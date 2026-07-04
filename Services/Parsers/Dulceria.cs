using FacturasApp.Models;
using System.Text.RegularExpressions;

namespace FacturasApp.Services.Parsers
{
    public partial class DulceriaParser : BaseParser
    {
        public override string Nombre => "Dulcería 17, S.L.";
        public override string Nif => "B11425964";

        protected override string[] Identificadores =>
            ["40.0015231", "40.080372"];

        [GeneratedRegex(@"(1-[\d]{7})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNumero();

        [GeneratedRegex(@"(.+)[\r\n]+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNombre();

        // Crear método GeneratedRegex específico
        [GeneratedRegex(@"([\d]{2}\.[\d]{3}\.[\d]{3}-.)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexNifEspecifico();

        // Sobrescribir usando el método generado
        protected override Regex RegexNif { get; } = RegexNifEspecifico();

        // Importes
        [GeneratedRegex(@"([,\.0-9]+)\s+([0124]+)%\s[,\.0-9]+[ \r\n]+([,\.0-9]+)[ \r\n]+Nº\.R\.S\.", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RegexImportes();

        public override Factura Parsear(string texto, string rutaArchivo, bool viaOcr)
        {
            var factura = CrearFacturaBase(rutaArchivo, viaOcr);

            factura.NumeroFactura = ExtraerGrupo(RegexNumero(), texto, 1);
            factura.Fecha = ExtraerFecha(texto);
            factura.Receptor.Nombre = ExtraerGrupo(RegexNombre(), texto, 1);
            factura.Receptor.NIF = ExtraerNif(texto);
            factura.BaseImponible = ExtraerDecimal(RegexImportes(), texto, 1);
            factura.PorcentajeIVA = ExtraerDecimal(RegexImportes(), texto, 2);
            factura.Total = ExtraerDecimal(RegexImportes(), texto, 3);
            factura.Estado = FacturaEstado.Determinar(factura);

            return factura;
        }
    }
}
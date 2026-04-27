namespace FacturasApp.Services.Parsers
{
    public class ParserFactoryExcel
    {
        private readonly List<IExcelInvoiceParser> _parsers;
        private readonly ExcelExtractor _genericExtractor = new();

        public ParserFactoryExcel()
        {
            _parsers = new List<IExcelInvoiceParser>
            {
                new DaviniaParser(),
                // Aquí añadirás nuevos parsers Excel en el futuro
            };
        }

        public (bool esEspecifico, IExcelInvoiceParser? parser) ObtenerParser(
            string rutaExcel)
        {
            var parser = _parsers.FirstOrDefault(p => p.PuedeParsar(rutaExcel));

            return parser != null
                ? (true, parser)
                : (false, null); // null = usar ExcelExtractor genérico
        }

        public IReadOnlyList<string> ParsersDisponibles =>
            _parsers.Select(p => p.Nombre).ToList();
    }
}
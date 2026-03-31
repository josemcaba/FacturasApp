using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public class ParserFactory
    {
        private readonly List<IInvoiceParser> _parsers;
        private readonly GenericParser _genericParser = new();

        public ParserFactory()
        {
            _parsers = new List<IInvoiceParser>
            {
                new AndresCazallaParser(),
                new MercadonaParser(),
                new EmasaParser(),
                new OnaCorpParser(),
                new LidlParser(),
                new PlenergyParser(),
            };
        }

        public IInvoiceParser ObtenerParser(string texto)
        {
            return _parsers.FirstOrDefault(p => p.PuedeParsar(texto))
                   ?? _genericParser;
        }

        public IReadOnlyList<string> ParsersDisponibles =>
            _parsers.Select(p => p.Nombre).ToList();
    }
}
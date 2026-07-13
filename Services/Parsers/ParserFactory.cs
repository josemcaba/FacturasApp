using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public class ParserFactory
    {
        private readonly List<IInvoiceParser> _parsers;
        private readonly GenericParser _genericParser = new();

        public ParserFactory()
        {
            _parsers =
            [
                new SarigaboParser(),
                new FiestaParser(),
                new DulceriaParser(),
                new OscarAriasParser(),
                new AndresCazalla(),
                new EMASA(),
                new FACCSA(),
                new MERCADONA(),
                new OnaCorpParser(),
                new LidlParser(),
                new PlenergyParser(),
                new GruasJuandiParser(),
                new MoncayoParser(),
                new GregorioArandaParser(),
                new IgnacioIbanezParser(),
                new PescaderiaMarengoParser(),
                new PescaderiaSalvadorParser(),
                new InversionesCerroPlomoParser(),
                new AmazonAwsParser(),
                new BixpeParser(),
            ];
        }

        public IInvoiceParser ObtenerParser(string texto)
        {
            return _parsers.FirstOrDefault(p => p.PuedeParsar(texto))
                   ?? _genericParser;
        }

        public IReadOnlyList<string> ParsersDisponibles =>
            [.. _parsers.Select(p => p.Nombre)];
    }
}
using DocumentFormat.OpenXml.Bibliography;
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
                new EsprinetParser(),
                new EurocabosParser(),
                new FobosParser(),
                new HostaliaParser(),
                new ComunicacionesCloudParser(),
                new InstantByteParser(),
                new PetroprixParser(),
                new SewanParser(),
                new VerisureParser(),
                new RangnyValenciaParser(),
                new EuroDepotParser(),
                new JuanLucasParser(),
                new CathedralSwParser(),
                new FasaworldParser(),
                new EnergiaXxiParser(),
                new CocinArteParser(),
                new TdSynnexParser(),
                new AutomotorPremiumParser(),
                new RyanairDacParser(),
                new NorelisParser(),
                new EasorParser(),
                new SimyoParser(),
                new IonosParser(),
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
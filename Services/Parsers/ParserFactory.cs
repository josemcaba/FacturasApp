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
                new AmazonAwsParser(),
                new AndresCazalla(),
                new AutomotorPremiumParser(),
                new BixpeParser(),
                new CathedralSwParser(),
                new CocinArteParser(),
                new ComunicacionesCloudParser(),
                new DisgoParser(),
                new DulceriaParser(),
                new EasorParser(),
                new EMASA(),
                new EnergiaXxiParser(),
                new EsprinetParser(),
                new EurocabosParser(),
                new EuroDepotParser(),
                new FACCSA(),
                new FasaworldParser(),
                new FiestaParser(),
                new FobosParser(),
                new GregorioArandaParser(),
                new GruasJuandiParser(),
                new HostaliaParser(),
                new IgnacioIbanezParser(),
                new InstantByteParser(),
                new InversionesCerroPlomoParser(),
                new IonosParser(),
                new JuanLucasParser(),
                new LidlParser(),
                new MERCADONA(),
                new MoncayoParser(),
                new NorelisParser(),
                new OnaCorpParser(),
                new OscarAriasParser(),
                new PescaderiaMarengoParser(),
                new PescaderiaSalvadorParser(),
                new PetroprixParser(),
                new PlenergyParser(),
                new RangnyValenciaParser(),
                new RyanairDacParser(),
                new SarigaboParser(),
                new SewanParser(),
                new SimyoParser(),
                new TdSynnexParser(),
                new TrigoricoParser(),
                new VerisureParser(),
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
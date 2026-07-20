using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public class ParserFactory
    {
        private readonly List<IInvoiceParser> _parsers;
        private readonly GenericParser _genericParser = new();
        private readonly ProveedorConfigService _configService = new();

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
                new CostaSolBalearParser(),
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
            // 1. Buscar en configuración XML (data-driven)
            var configProveedor = _configService.ObtenerPorIdentificadores(texto);
            if (configProveedor != null)
                return new DataDrivenParser(configProveedor);

            // 2. Fallback a parsers code-behind
            var parserCode = _parsers.FirstOrDefault(p => p.PuedeParsar(texto));
            if (parserCode != null)
                return parserCode;

            // 3. Último recurso: parser genérico
            return _genericParser;
        }

        public IReadOnlyList<string> ParsersDisponibles
        {
            get
            {
                var nombres = _parsers.Select(p => p.Nombre).ToList();
                nombres.AddRange(_configService.ObtenerNombresProveedores());
                return [.. nombres.Distinct().OrderBy(n => n)];
            }
        }
    }
}

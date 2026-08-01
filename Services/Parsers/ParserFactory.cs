using FacturasApp.Models;
using FacturasApp.Models.EmisoresConfig;

namespace FacturasApp.Services.Parsers
{
    public class ParserFactory
    {
        private readonly List<IInvoiceParser> _parsers;
        private readonly GenericParser _genericParser = new();
        private readonly ConfiguracionEmisores _configuracionEmisores = new();

        public ParserFactory()
        {
            _parsers =
            [
                new AmazonAwsParser(),
                new AutomotorPremiumParser(),
                new BixpeParser(),
                new CathedralSwParser(),
                new CocinArteParser(),
                new ComunicacionesCloudParser(),
                new CostaSolBalearParser(),
                new DisgoParser(),
                // new DulceriaParser(),
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
                // new TrigoricoParser(),
                new VerisureParser(),
            ];
        }

        public IInvoiceParser ObtenerParser(string texto)
        {
            var configs = _configuracionEmisores.CargarTodos();

            // 1. XML configs con identificadores → prioridad
            foreach (var config in configs.Values)
            {
                if (config.Identificadores is { Count: > 0 } &&
                    config.Identificadores.All(id =>
                        texto.Contains(id, StringComparison.OrdinalIgnoreCase)))
                {
                    return new ConfigurableParserEngine(config);
                }
            }

            // 2. Hardcoded C# parsers
            var hardcoded = _parsers.FirstOrDefault(p => p.PuedeParsar(texto));
            if (hardcoded != null)
                return hardcoded;

            // 3. Fallback: General.xml (editable por el usuario)
            if (configs.TryGetValue("General", out var generalConfig))
                return new ConfigurableParserEngine(generalConfig);

            // 4. Fallback último: GenericParser C#
            return _genericParser;
        }

        public IReadOnlyList<string> ParsersDisponibles
        {
            get
            {
                var nombres = _parsers.Select(p => p.Nombre).ToList();
                foreach (var config in _configuracionEmisores.CargarTodos().Values)
                {
                    if (!string.IsNullOrEmpty(config.Nombre) && !nombres.Contains(config.Nombre))
                        nombres.Add(config.Nombre);
                }
                return nombres;
            }
        }
    }
}
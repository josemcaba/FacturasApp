using FacturasApp.Core.Models;
using FacturasApp.Core.Models.EmisoresConfig;

namespace FacturasApp.Core.Services.Parsers
{
    public class ParserFactory
    {
        private readonly ConfiguracionEmisores _configuracionEmisores = new();

        public IInvoiceParser? ObtenerParser(string texto) =>
            ObtenerParser(texto, null);

        /// <summary>
        /// Devuelve el parser XML (<see cref="ConfigurableParserEngine"/>) del emisor
        /// identificado por sus &lt;Identificadores&gt;, o <c>null</c> si ningún XML
        /// identifica el documento: no hay parsers C# de respaldo, la extracción es
        /// XML-only y sin config el llamante debe marcar el documento en Error.
        /// </summary>
        public IInvoiceParser? ObtenerParser(string texto, EmisorConfig? configPreferida)
        {
            var configs = _configuracionEmisores.CargarTodos();

            // 0. Config preferida (en edición en la UI) con identificadores → prioridad
            if (configPreferida != null &&
                CoincideConIdentificadores(configPreferida, texto))
            {
                return new ConfigurableParserEngine(configPreferida);
            }

            // 1. XML configs con identificadores → motor XML
            foreach (var config in configs.Values)
            {
                if (CoincideConIdentificadores(config, texto))
                {
                    return new ConfigurableParserEngine(config);
                }
            }

            // 2. Fallback XML: General.xml (si el usuario lo ha creado, cubre
            //    cualquier documento no identificado)
            if (configs.TryGetValue("General", out var generalConfig))
                return new ConfigurableParserEngine(generalConfig);

            // 3. Sin config XML → null: no se extrae nada (el pipeline marca Error)
            return null;
        }

        private static bool CoincideConIdentificadores(EmisorConfig config, string texto) =>
            config.Identificadores is { Count: > 0 } &&
            config.Identificadores.All(id =>
                texto.Contains(id, StringComparison.OrdinalIgnoreCase));

        public IReadOnlyList<string> ParsersDisponibles =>
            [.. _configuracionEmisores.CargarTodos().Values
                .Where(config => !string.IsNullOrEmpty(config.Nombre))
                .Select(config => config.Nombre)];
    }
}

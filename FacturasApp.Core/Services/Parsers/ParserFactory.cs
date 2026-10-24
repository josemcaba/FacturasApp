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

            // 1. XML configs con identificadores → motor XML. La copia en caché
            //    de la config preferida (la que se está editando en la UI) no
            //    compite: refleja la última versión guardada, no la edición actual.
            foreach (var config in configs.Values)
            {
                if (configPreferida != null &&
                    string.Equals(config.Nif?.Trim(), configPreferida.Nif?.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

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

        /// <summary>
        /// Devuelve la config de TODOS los XML cuyos identificadores coinciden
        /// con el texto (no sólo el primero, y sin importar el orden de ficheros):
        /// lo usa la UI de Gestión de Emisores para avisar cuando otra config
        /// distinta de la que se está editando identifica también el documento.
        /// </summary>
        public IReadOnlyList<EmisorConfig> ObtenerConfigsQueIdentifican(string texto) =>
            _configuracionEmisores.CargarTodos().Values
                .Where(config => CoincideConIdentificadores(config, texto))
                .ToList();

        public IReadOnlyList<string> ParsersDisponibles =>
            [.. _configuracionEmisores.CargarTodos().Values
                .Where(config => !string.IsNullOrEmpty(config.Nombre))
                .Select(config => config.Nombre)];
    }
}

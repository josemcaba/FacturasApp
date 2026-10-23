using System.Collections.Concurrent;
using FacturasApp.Core.Models;

namespace FacturasApp.Core.Services
{
    /// <summary>
    /// Verifica el NIF del receptor de una factura con el mismo criterio que el
    /// botón «Verificar NIF» de los emisores (GestionEmisoresForm): primero el
    /// NIF español en local (NifValidator, sin red) y sólo si no lo es se
    /// consulta VIES para los NIF-IVA comunitarios.
    /// </summary>
    /// <remarks>
    /// La caché de consultas VIES dura lo que una sesión de procesado: se
    /// limpia al empezar cada ProcesarMixto (una pulsación del botón
    /// «Procesar» de la GUI). Dentro de la sesión NO hay reintentos: si VIES
    /// no está disponible, ese NIF queda como "NoDisponible" para el resto
    /// del lote (sus facturas pasan a Revisar) sin volver a consultar la red.
    /// </remarks>
    public static class NifVerificador
    {
        // Resultados VIES cacheados por NIF normalizado durante la sesión.
        private static readonly ConcurrentDictionary<string, ViesResultado> Cache = new();

        /// <summary>
        /// Devuelve el estado del NIF con valores equivalentes a
        /// EmisorConfig.NifEstado: "Valido", "Invalido" o "NoDisponible".
        /// Detalle explica el fallo (null cuando basta el mensaje genérico
        /// «NIF del Cliente no válido»).
        /// </summary>
        public static (string Estado, string? Detalle) Verificar(string? nif)
        {
            var limpio = (nif ?? string.Empty).Trim();

            // NIF español (DNI/NIE/CIF): se resuelve en local, sin la red.
            if (NifValidator.ValidarNif(limpio))
                return ("Valido", null);

            // Los NIF británicos (GB) dejaron de existir en VIES el 1/1/2021
            // (Brexit): deben comprobarse en la HMRC británica.
            if (ViesValidator.ObtenerPrefijoPais(limpio) == "GB")
                return ("NoDisponible",
                    "los NIF británicos (GB) dejaron de verificarse en VIES " +
                    "el 1/1/2021 (Brexit); comprobar en la HMRC británica");

            if (ViesValidator.EsNifComunitario(limpio))
            {
                var resultado = ConsultarVies(limpio);
                if (resultado.Estado == "Valido")
                    return ("Valido", null);
                if (resultado.Estado == "Invalido")
                    // Con CodigoError (p. ej. INVALID_INPUT) el motivo real es
                    // el propio servicio rechazando el número, no una ausencia
                    // de registro: se explica el código en lugar del genérico.
                    return ("Invalido", resultado.CodigoError != null
                        ? resultado.MensajeError
                        : "no consta como válido en VIES para operaciones intracomunitarias");
                return ("NoDisponible",
                    $"VIES no disponible: {resultado.MensajeError ?? "sin respuesta del servicio"}");
            }

            // No es ni un NIF español ni un NIF-IVA comunitario.
            return ("Invalido", null);
        }

        /// <summary>Vacía la caché VIES: cada sesión de procesado consulta de
        /// nuevo los NIF de receptor, como si fuera la primera vez.</summary>
        public static void LimpiarCache() => Cache.Clear();

        // La primera factura de un lote consulta VIES; las siguientes con el
        // mismo receptor reutilizan el resultado guardado (también si la
        // consulta no estuvo disponible: sin reintentos dentro de la sesión).
        private static ViesResultado ConsultarVies(string nif)
        {
            // Misma normalización que ViesValidator.Normalizar (el servicio ya
            // la aplica al consultar; aquí sólo identifica la clave de caché).
            var clave = new string(nif.ToUpperInvariant()
                .Where(c => !char.IsWhiteSpace(c) && c != '-')
                .ToArray());
            return Cache.GetOrAdd(clave, _ => ViesValidator.Verificar(nif));
        }
    }
}

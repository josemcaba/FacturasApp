using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FacturasApp.Core.Models;

namespace FacturasApp.Core.Services
{
    /// <summary>
    /// Consulta el servicio oficial VIES (VAT Information Exchange System) de la
    /// Comisión Europea para verificar NIF-IVA comunitarios. Usa el interfaz SOAP
    /// documentado (checkVatService.wsdl), sin paquetes externos: sólo HttpClient
    /// y XML de la BCL.
    /// </summary>
    /// <remarks>
    /// VIES es sólo un enrutador hacia los registros nacionales: cada estado
    /// miembro puede caerse de forma independiente (los lunes madrugada hay
    /// mantenimiento programado), por eso los fallos de servicio se devuelven
    /// como "no disponible" y nunca como "inválido".
    /// Los NIF con prefijo GB dejaron de existir en VIES el 1/1/2021 (Brexit):
    /// deben verificarse en la HMRC británica. Los de Irlanda del Norte (XI)
    /// siguen en el sistema.
    /// </remarks>
    public class ViesValidator
    {
        private const string UrlServicio =
            "https://ec.europa.eu/taxation_customs/vies/services/checkVatService";

        private static readonly XNamespace NsTipos =
            "urn:ec.europa.eu:taxud:vies:services:checkVat:types";
        private static readonly XNamespace NsSoap =
            "http://schemas.xmlsoap.org/soap/envelope/";

        // Un único cliente compartido para toda la app: crear uno por consulta
        // agota el puerto de salida. VIES responde normalmente en 1-2 s; el
        // timeout cubre los estados miembros caídos (acaban en fault TIMEOUT).
        private static readonly HttpClient Http = new()
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        // Formato del vatNumber según el WSDL: [0-9A-Za-z\+\*\.]{2,12}
        // (sin el prefijo de país, que viaja aparte en countryCode).
        private static readonly Regex FormatoNumero =
            new(@"^[0-9A-Z\+\*\.]{2,12}$", RegexOptions.Compiled);

        // Estados miembros de la UE + XI (Irlanda del Norte, post-Brexit).
        // Grecia es EL en VIES, nunca GR.
        private static readonly HashSet<string> PaisesVies = new()
        {
            "AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "EL", "ES", "FI",
            "FR", "HR", "HU", "IE", "IT", "LT", "LU", "LV", "MT", "NL", "PL",
            "PT", "RO", "SE", "SI", "SK", "XI"
        };

        /// <summary>
        /// Devuelve el prefijo de país del NIF (normalizado: GR → EL) o null si
        /// el NIF no empieza por un prefijo de estado miembro de VIES.
        /// </summary>
        public static string? ObtenerPrefijoPais(string? nif)
        {
            var limpio = Normalizar(nif);
            if (limpio.Length < 2 || !limpio[..2].All(char.IsLetter))
                return null;

            var prefijo = limpio[..2];
            return prefijo == "GR" ? "EL" : prefijo;
        }

        /// <summary>true si el NIF empieza por el prefijo de un estado miembro
        /// (o Irlanda del Norte): es un NIF-IVA comunitario verificable en VIES.
        /// Los GB también llevan prefijo británico, pero ya no son verificables.
        /// </summary>
        public static bool EsNifComunitario(string? nif)
        {
            var prefijo = ObtenerPrefijoPais(nif);
            return prefijo != null && PaisesVies.Contains(prefijo);
        }

        /// <summary>
        /// Separa un NIF en (país, número) para la consulta. Con prefijo
        /// comunitario se separa y normaliza; sin él se asume España (ES).
        /// Espacios, guiones y minúsculas no estorban.
        /// </summary>
        public static (string Pais, string Numero) ParsearNif(string? nif)
        {
            var limpio = Normalizar(nif);
            var prefijo = ObtenerPrefijoPais(limpio);
            if (prefijo != null && (PaisesVies.Contains(prefijo) || prefijo == "GB"))
                return (prefijo, limpio[2..]);
            return ("ES", limpio);
        }

        /// <summary>
        /// Consulta VIES y devuelve el resultado. Nunca lanza por fallo de red o
        /// de servicio: esos casos llegan como Disponible=false con su código.
        /// </summary>
        public static async Task<ViesResultado> VerificarAsync(string? nif)
        {
            var (pais, numero) = ParsearNif(nif);
            var resultado = new ViesResultado { FechaConsulta = DateTime.Now };

            // Corta antes de ir a la red lo que VIES rechazaría igualmente
            // (p. ej. el NIF de marcador "NUEVO_NIF", con guion bajo).
            if (!FormatoNumero.IsMatch(numero))
            {
                resultado.Disponible = true;
                resultado.CodigoError = "FORMATO_NO_VALIDO";
                return resultado;
            }

            var envelope = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(NsSoap + "Envelope",
                    new XElement(NsSoap + "Header"),
                    new XElement(NsSoap + "Body",
                        new XElement(NsTipos + "checkVat",
                            new XElement(NsTipos + "countryCode", pais),
                            new XElement(NsTipos + "vatNumber", numero)))));

            HttpResponseMessage respuesta;
            try
            {
                using var contenido = new StringContent(
                    envelope.ToString(), Encoding.UTF8, "text/xml");
                respuesta = await Http.PostAsync(UrlServicio, contenido)
                    .ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                resultado.CodigoError = "TIMEOUT";
                return resultado;
            }
            catch (HttpRequestException)
            {
                resultado.CodigoError = "SIN_CONEXION";
                return resultado;
            }

            using (respuesta)
            {
                var cuerpo = await respuesta.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                if (!respuesta.IsSuccessStatusCode)
                {
                    resultado.CodigoError =
                        respuesta.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable
                            ? "SERVICE_UNAVAILABLE"
                            : $"HTTP_{(int)respuesta.StatusCode}";
                    return resultado;
                }

                XDocument doc;
                try
                {
                    doc = XDocument.Parse(cuerpo);
                }
                catch
                {
                    resultado.CodigoError = "RESPUESTA_INVALIDA";
                    return resultado;
                }

                // VIES devuelve los faults con HTTP 200 dentro del envelope SOAP.
                var fault = doc.Descendants()
                    .FirstOrDefault(e => e.Name.LocalName == "Fault");
                if (fault != null)
                {
                    var codigo = fault.Elements()
                        .FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value
                        ?? "DESCONOCIDO";

                    // INVALID_INPUT es el propio servicio rechazando el número
                    // enviado: el NIF no vale, pero el servicio sí funcionó.
                    if (codigo == "INVALID_INPUT")
                    {
                        resultado.Disponible = true;
                        resultado.CodigoError = codigo;
                        return resultado;
                    }

                    resultado.CodigoError = codigo;
                    return resultado;
                }

                var cuerpoRespuesta = doc.Descendants(NsTipos + "checkVatResponse")
                    .FirstOrDefault()
                    ?? doc.Descendants()
                        .FirstOrDefault(e => e.Name.LocalName == "checkVatResponse");
                if (cuerpoRespuesta == null)
                {
                    resultado.CodigoError = "RESPUESTA_INVALIDA";
                    return resultado;
                }

                resultado.Disponible = true;
                resultado.Valido = string.Equals(
                    Elemento(cuerpoRespuesta, "valid"), "true",
                    StringComparison.OrdinalIgnoreCase);
                resultado.Nombre = Elemento(cuerpoRespuesta, "name");
                resultado.Direccion = Elemento(cuerpoRespuesta, "address");
                // requestDate lo ignora: se persiste el momento local de consulta.
                return resultado;
            }
        }

        /// <summary>
        /// Versión síncrona de <see cref="VerificarAsync"/> para el procesado
        /// en segundo plano (FacturaEstado corre dentro de un Task.Run, sin
        /// contexto de sincronización: no hay riesgo de deadlock). No debe
        /// llamarse desde el hilo de la UI, que sí se bloquearía hasta 20 s.
        /// </summary>
        public static ViesResultado Verificar(string? nif) =>
            VerificarAsync(nif).GetAwaiter().GetResult();

        /// <summary>Quita espacios, guiones y pasa a mayúsculas (el campo
        /// CharacterCasing.Upper de la GUI ya lo hace, pero el servicio no
        /// debe depender de eso).</summary>
        private static string Normalizar(string? nif) => new string(
            (nif ?? string.Empty).Trim().ToUpperInvariant()
                .Where(c => !char.IsWhiteSpace(c) && c != '-')
                .ToArray());

        /// <summary>Valor de un hijo por nombre local; vacío o "---" (lo usa VIES
        /// como marcador de "sin dato") → null.</summary>
        private static string? Elemento(XElement contenedor, string nombreLocal)
        {
            var valor = contenedor.Elements()
                .FirstOrDefault(e => e.Name.LocalName == nombreLocal)?.Value;
            return string.IsNullOrWhiteSpace(valor) || valor.Trim() == "---"
                ? null
                : valor.Trim();
        }
    }
}

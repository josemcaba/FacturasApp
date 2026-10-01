using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Reflection;
using FacturasApp.Core.Models.EmisoresConfig;

namespace FacturasApp.Core.Services;

public class ConfiguracionEmisores
{
    private const string PrefijoRecurso = "FacturasApp.Core.Data.Emisores.";

    private static readonly string RutaDirectorio = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FacturasApp", "Emisores");

    private static Dictionary<string, EmisorConfig>? _cache;

    private readonly XmlSerializer _serializer = new(typeof(EmisorConfig));

    public Dictionary<string, EmisorConfig> CargarTodos()
    {
        if (_cache != null)
            return _cache;

        Directory.CreateDirectory(RutaDirectorio);
        ExtraerEmisoresPorDefecto();

        var emisores = new Dictionary<string, EmisorConfig>(StringComparer.OrdinalIgnoreCase);

        foreach (var ruta in Directory.GetFiles(RutaDirectorio, "*.xml"))
        {
            try
            {
                using var stream = File.OpenRead(ruta);
                if (_serializer.Deserialize(stream) is EmisorConfig config)
                {
                    var clave = Path.GetFileNameWithoutExtension(ruta);
                    emisores[clave] = config;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"✗ Error cargando {Path.GetFileName(ruta)}: {ex.Message}");
            }
        }

        _cache = emisores;
        return emisores;
    }

    public EmisorConfig? ObtenerPorNif(string nif)
    {
        var todos = CargarTodos();
        return todos.TryGetValue(nif, out var config) ? config : null;
    }

    public void Guardar(EmisorConfig config, string? nifAnterior = null)
        => Guardar(config, nifAnterior, RutaDirectorio);

    /// <param name="directorio">Carpeta destino; el sobre-concreto permite probar
    /// el guardado sin escribir en el %APPDATA% real.</param>
    public void Guardar(EmisorConfig config, string? nifAnterior, string directorio)
    {
        var nif = SanitizarNombreArchivo(config.Nif);
        var ruta = Path.Combine(directorio, $"{nif}.xml");
        var sentinel = Path.Combine(directorio, $"{nif}.eliminado");

        // Delete old file if NIF changed (for existing emitters)
        if (nifAnterior != null && !string.Equals(nifAnterior, config.Nif, StringComparison.OrdinalIgnoreCase))
        {
            var nifAnt = SanitizarNombreArchivo(nifAnterior);
            var rutaAnt = Path.Combine(directorio, $"{nifAnt}.xml");
            if (File.Exists(rutaAnt))
                File.Delete(rutaAnt);
        }

        if (File.Exists(sentinel))
            File.Delete(sentinel);

        Directory.CreateDirectory(directorio);

        // El timestamp de publicación sólo se incrementa si el contenido cambió de
        // verdad: así editar en la GUI equivale a "nueva versión", mientras que un
        // guardado sin cambios no provoca sobrescrituras inútiles en los clientes.
        // Hay que leer el disco ANTES de File.Create, que trunca el fichero.
        var enDisco = File.Exists(ruta) ? File.ReadAllText(ruta) : null;
        var haCambiado = enDisco == null
            || ContenidoSinVersion(Serializar(config)) != ContenidoSinVersion(enDisco);

        if (haCambiado)
        {
            // Math.Max: si el reloj de alguna máquina va atrasado, nunca se graba un
            // valor menor que el que ya está en disco, que no se propagaría nunca.
            var ahora = long.Parse(DateTime.Now.ToString("yyyyMMddHHmm"));
            config.Version = Math.Max(ahora, config.Version + 1);
        }

        using var stream = File.Create(ruta);
        _serializer.Serialize(stream, config);

        _cache ??= new Dictionary<string, EmisorConfig>(StringComparer.OrdinalIgnoreCase);
        _cache[config.Nif] = config;
    }

    public void Eliminar(string nif)
    {
        var nifArchivo = SanitizarNombreArchivo(nif);
        var ruta = Path.Combine(RutaDirectorio, $"{nifArchivo}.xml");
        var sentinel = Path.Combine(RutaDirectorio, $"{nifArchivo}.eliminado");

        if (File.Exists(ruta))
            File.Delete(ruta);

        File.WriteAllText(sentinel, string.Empty);

        _cache?.Remove(nif);
    }

    public void Recargar()
    {
        _cache = null;
    }

    private void ExtraerEmisoresPorDefecto() => Extraer(RutaDirectorio);

    /// <summary>
    /// Extrae los emisores embebidos en el ensamblado a <paramref name="directorio"/>.
    /// </summary>
    /// <remarks>
    /// Regla por emisor:
    /// <list type="number">
    /// <item>Sentinel <c>.eliminado</c>: el usuario lo borró → no se resucita.</item>
    /// <item>No existe el destino → se escribe (instalación limpia).</item>
    /// <item>Versión publicada &gt; versión local → copia previa + sobrescritura.</item>
    /// <item>Cualquier otro caso → se conserva la copia local (igual o más reciente).</item>
    /// </list>
    /// El directorio va como parámetro para poder ejercitarlo sin tocar el %APPDATA% real.
    /// </remarks>
    public void Extraer(string directorio)
    {
        var ensamblado = Assembly.GetExecutingAssembly();
        var recursos = ensamblado.GetManifestResourceNames()
            .Where(r => r.StartsWith(PrefijoRecurso, StringComparison.Ordinal)
                     && r.EndsWith(".xml", StringComparison.Ordinal));

        Directory.CreateDirectory(directorio);

        foreach (var recurso in recursos)
        {
            var nombreArchivo = recurso[PrefijoRecurso.Length..];
            var sinExtension = Path.GetFileNameWithoutExtension(nombreArchivo);
            var rutaDestino = Path.Combine(directorio, nombreArchivo);
            var sentinel = Path.Combine(directorio, $"{sinExtension}.eliminado");

            if (File.Exists(sentinel))
                continue;

            using var stream = ensamblado.GetManifestResourceStream(recurso);
            if (stream == null) continue;

            // Los bytes se leen una sola vez: sirven para medir la versión y para escribir.
            using var memoria = new MemoryStream();
            stream.CopyTo(memoria);
            var bytes = memoria.ToArray();
            var versionPublicada = LeerVersion(bytes);

            if (!File.Exists(rutaDestino))
            {
                File.WriteAllBytes(rutaDestino, bytes);
                System.Diagnostics.Debug.WriteLine(
                    $"✓ Extraído emisor por defecto: {nombreArchivo}");
                continue;
            }

            var versionLocal = LeerVersion(rutaDestino);
            if (versionPublicada <= versionLocal)
                continue; // la copia local es igual o más reciente: no se toca

            // Se respalda lo que se va a pisar: una edición local no se pierde sin rastro.
            var previa = Path.Combine(directorio, $"{sinExtension}.previa.xml.bak");
            File.Copy(rutaDestino, previa, overwrite: true);
            File.WriteAllBytes(rutaDestino, bytes);

            System.Diagnostics.Debug.WriteLine(
                $"↻ Emisor actualizado: {nombreArchivo} v{versionLocal} → v{versionPublicada} " +
                $"(copia previa en {Path.GetFileName(previa)})");
        }
    }

    private static long LeerVersion(byte[] datos)
    {
        try
        {
            return LeerVersion(XDocument.Parse(Encoding.UTF8.GetString(datos)));
        }
        catch
        {
            return 1; // XML ilegible → 1, para no pisar nada por accidente
        }
    }

    private static long LeerVersion(string ruta)
    {
        try
        {
            return LeerVersion(XDocument.Load(ruta));
        }
        catch
        {
            return 1;
        }
    }

    private static long LeerVersion(XDocument documento)
    {
        var texto = documento.Root?.Element("Version")?.Value;
        return long.TryParse(texto, out var version) && version > 0 ? version : 1;
    }

    private string Serializar(EmisorConfig config)
    {
        using var memoria = new MemoryStream();
        _serializer.Serialize(memoria, config);
        return Encoding.UTF8.GetString(memoria.ToArray());
    }

    /// <summary>
    /// Deja el XML sin el elemento &lt;Version&gt; y normaliza fin de línea, para
    /// comparar el contenido real ignorando la versión y los CRLF/LF del editor.
    /// </summary>
    private static string ContenidoSinVersion(string xml)
    {
        var sinVersion = Regex.Replace(xml, "<Version>.*?</Version>", string.Empty,
            RegexOptions.Singleline);
        return sinVersion.Replace("\r\n", "\n");
    }

    private static string SanitizarNombreArchivo(string nif)
    {
        var invalidos = Path.GetInvalidFileNameChars();
        return string.Concat(nif.Where(c => !invalidos.Contains(c))).Trim();
    }
}

using System.Xml.Serialization;
using System.Reflection;
using FacturasApp.Models.EmisoresConfig;

namespace FacturasApp.Services;

public class ConfiguracionEmisores
{
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

    public void Guardar(EmisorConfig config)
    {
        var nif = SanitizarNombreArchivo(config.Nif);
        var ruta = Path.Combine(RutaDirectorio, $"{nif}.xml");

        Directory.CreateDirectory(RutaDirectorio);
        using var stream = File.Create(ruta);
        _serializer.Serialize(stream, config);

        _cache ??= new Dictionary<string, EmisorConfig>(StringComparer.OrdinalIgnoreCase);
        _cache[config.Nif] = config;
    }

    public void Eliminar(string nif)
    {
        var nifArchivo = SanitizarNombreArchivo(nif);
        var ruta = Path.Combine(RutaDirectorio, $"{nifArchivo}.xml");

        if (File.Exists(ruta))
            File.Delete(ruta);

        _cache?.Remove(nif);
    }

    public void Recargar()
    {
        _cache = null;
    }

    private void ExtraerEmisoresPorDefecto()
    {
        var ensamblado = Assembly.GetExecutingAssembly();
        var recursos = ensamblado.GetManifestResourceNames()
            .Where(r => r.StartsWith("FacturasApp.Data.Emisores.") && r.EndsWith(".xml"));

        foreach (var recurso in recursos)
        {
            var nombreArchivo = recurso.Replace("FacturasApp.Data.Emisores.", "");
            var rutaDestino = Path.Combine(RutaDirectorio, nombreArchivo);

            if (File.Exists(rutaDestino))
                continue;

            using var stream = ensamblado.GetManifestResourceStream(recurso);
            if (stream == null) continue;

            Directory.CreateDirectory(RutaDirectorio);
            using var fileStream = File.Create(rutaDestino);
            stream.CopyTo(fileStream);

            System.Diagnostics.Debug.WriteLine(
                $"✓ Extraído emisor por defecto: {nombreArchivo}");
        }
    }

    private static string SanitizarNombreArchivo(string nif)
    {
        var invalidos = Path.GetInvalidFileNameChars();
        return string.Concat(nif.Where(c => !invalidos.Contains(c))).Trim();
    }
}

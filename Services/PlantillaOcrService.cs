using FacturasApp.Models;
using System.Xml.Serialization;

namespace FacturasApp.Services
{
    public class PlantillaOcrService
    {
        // ── Ubicación en AppData (modificable por usuario) ──
        private static readonly string RutaDirectorio = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FacturasApp");

        private static readonly string RutaXmlUsuario = Path.Combine(
            RutaDirectorio, "plantillas_ocr.xml");

        // ── Ubicación del archivo de la aplicación ──
        private static string ObtenerRutaXmlAplicacion()
        {
            // Obtener la ruta base donde se ejecuta la aplicación
            // En ClickOnce: caché local de ClickOnce
            // En debug: carpeta bin\Debug
            string rutaApp = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(rutaApp, "Data", "plantillas_ocr.xml");
        }

        private readonly XmlSerializer _serializer =
            new(typeof(PlantillasOcrColeccion));

        // ── Carga ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Carga las plantillas. En primer inicio copia el archivo predefinido 
        /// a AppData. En actualizaciones, sobrescribe el archivo del usuario 
        /// con la versión nueva de la aplicación.
        /// </summary>
        public PlantillasOcrColeccion Cargar()
        {
            // Asegurar que el directorio en AppData existe
            Directory.CreateDirectory(RutaDirectorio);

            // Estrategia de actualización: si el archivo de la aplicación es más reciente,
            // sobrescribir el del usuario (para actualizaciones)
            ActualizarSiNecesario();

            // Intentar cargar del directorio de usuario (AppData)
            if (File.Exists(RutaXmlUsuario))
            {
                try
                {
                    using var stream = new FileStream(RutaXmlUsuario, FileMode.Open);
                    return _serializer.Deserialize(stream)
                           as PlantillasOcrColeccion
                           ?? new PlantillasOcrColeccion();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error cargando plantillas: {ex.Message}");
                    return new PlantillasOcrColeccion();
                }
            }

            // Si no existe en AppData, retornar colección vacía
            return new PlantillasOcrColeccion();
        }

        // ── Actualización automática en cada inicio ──────────────────────────

        /// <summary>
        /// Si el archivo en la aplicación es más reciente que el del usuario,
        /// lo sobrescribe. Esto permite que las actualizaciones de ClickOnce
        /// actualicen también las plantillas predefinidas.
        /// </summary>
        private void ActualizarSiNecesario()
        {
            string rutaXmlAplicacion = ObtenerRutaXmlAplicacion();

            if (!File.Exists(rutaXmlAplicacion))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Archivo de plantillas predefinido no encontrado en: {rutaXmlAplicacion}");
                return;
            }

            try
            {
                // Si no existe el archivo del usuario, copiar el de la aplicación
                if (!File.Exists(RutaXmlUsuario))
                {
                    File.Copy(rutaXmlAplicacion, RutaXmlUsuario, overwrite: false);
                    System.Diagnostics.Debug.WriteLine(
                        $"Plantillas iniciales copiadas a: {RutaXmlUsuario}");
                    return;
                }

                // Comparar fechas: si la aplicación tiene una versión más reciente, actualizar
                var infoApp = new FileInfo(rutaXmlAplicacion);
                var infoUsuario = new FileInfo(RutaXmlUsuario);

                if (infoApp.LastWriteTimeUtc > infoUsuario.LastWriteTimeUtc)
                {
                    File.Copy(rutaXmlAplicacion, RutaXmlUsuario, overwrite: true);
                    System.Diagnostics.Debug.WriteLine(
                        $"Plantillas actualizadas desde: {rutaXmlAplicacion}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando plantillas: {ex.Message}");
            }
        }

        // ── Guarda ────────────────────────────────────────────────────────────

        public void Guardar(PlantillasOcrColeccion coleccion)
        {
            try
            {
                Directory.CreateDirectory(RutaDirectorio);

                using var stream = new FileStream(RutaXmlUsuario,
                    FileMode.Create, FileAccess.Write);

                _serializer.Serialize(stream, coleccion);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando plantillas: {ex.Message}");
            }
        }

        // ── Operaciones sobre plantillas ──────────────────────────────────────

        public PlantillaOcr? ObtenerPorEmisor(string nombreEmisor)
        {
            var coleccion = Cargar();
            return coleccion.Plantillas.FirstOrDefault(p =>
                p.Emisor.Equals(nombreEmisor,
                    StringComparison.OrdinalIgnoreCase));
        }

        public void GuardarPlantilla(PlantillaOcr plantilla)
        {
            var coleccion = Cargar();

            // Reemplazamos si ya existe una para este emisor
            var existente = coleccion.Plantillas.FirstOrDefault(p =>
                p.Emisor.Equals(plantilla.Emisor,
                    StringComparison.OrdinalIgnoreCase));

            if (existente != null)
                coleccion.Plantillas.Remove(existente);

            coleccion.Plantillas.Add(plantilla);
            Guardar(coleccion);
        }

        public void EliminarPlantilla(string nombreEmisor)
        {
            var coleccion = Cargar();
            coleccion.Plantillas.RemoveAll(p =>
                p.Emisor.Equals(nombreEmisor,
                    StringComparison.OrdinalIgnoreCase));
            Guardar(coleccion);
        }

        public List<string> ObtenerEmisoresConPlantilla()
        {
            return Cargar().Plantillas
                .Select(p => p.Emisor)
                .OrderBy(e => e)
                .ToList();
        }

        // ── Diagnóstico (útil para debugging) ────────────────────────────────

        /// <summary>
        /// Retorna información sobre las rutas de los archivos para debugging.
        /// </summary>
        public (string rutaApp, string rutaUsuario, bool existeApp, bool existeUsuario)
            ObtenerInfoRutas()
        {
            string rutaApp = ObtenerRutaXmlAplicacion();
            return (
                rutaApp: rutaApp,
                rutaUsuario: RutaXmlUsuario,
                existeApp: File.Exists(rutaApp),
                existeUsuario: File.Exists(RutaXmlUsuario)
            );
        }
    }
}
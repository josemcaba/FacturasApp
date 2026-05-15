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
            string rutaApp = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(rutaApp, "Data", "plantillas_ocr.xml");
        }

        private readonly XmlSerializer _serializer =
            new(typeof(PlantillasOcrColeccion));

        // ── Carga ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Carga las plantillas. En primer inicio copia el archivo predefinido 
        /// a AppData. En actualizaciones, sobrescribe si la app tiene versión más reciente.
        /// </summary>
        public PlantillasOcrColeccion Cargar()
        {
            // Asegurar que el directorio en AppData existe
            Directory.CreateDirectory(RutaDirectorio);

            // Actualizar plantillas si hay nueva versión
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
        /// Compara la fecha del archivo de la aplicación con el del usuario.
        /// Si el de la aplicación es más reciente (nueva publicación),
        /// sobrescribe el del usuario con la nueva versión.
        /// 
        /// Esto es más confiable que comparar versiones de ensamblado,
        /// especialmente con ClickOnce que no siempre actualiza las fechas correctamente.
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

                // Obtener fechas de modificación
                var infoApp = new FileInfo(rutaXmlAplicacion);
                var infoUsuario = new FileInfo(RutaXmlUsuario);

                // Comparar con tolerancia de 1 segundo para evitar problemas de precisión
                TimeSpan diferencia = infoApp.LastWriteTimeUtc - infoUsuario.LastWriteTimeUtc;

                System.Diagnostics.Debug.WriteLine(
                    $"App: {infoApp.LastWriteTimeUtc:yyyy-MM-dd HH:mm:ss}, " +
                    $"Usuario: {infoUsuario.LastWriteTimeUtc:yyyy-MM-dd HH:mm:ss}, " +
                    $"Diferencia: {diferencia.TotalSeconds:F1}s");

                // Si la app tiene una versión más reciente (al menos 1 segundo), actualizar
                if (diferencia.TotalSeconds > 1.0)
                {
                    File.Copy(rutaXmlAplicacion, RutaXmlUsuario, overwrite: true);
                    System.Diagnostics.Debug.WriteLine(
                        $"Plantillas actualizadas desde la aplicación " +
                        $"({Math.Abs(diferencia.TotalSeconds):F1}s más nueva)");
                }
                else if (diferencia.TotalSeconds < -1.0)
                {
                    // El archivo del usuario es más reciente
                    System.Diagnostics.Debug.WriteLine(
                        $"Archivo del usuario es más reciente, no se actualiza");
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
        public (string rutaApp, string rutaUsuario, bool existeApp, bool existeUsuario, DateTime? fechaApp, DateTime? fechaUsuario)
            ObtenerInfoRutas()
        {
            string rutaApp = ObtenerRutaXmlAplicacion();
            DateTime? fechaApp = File.Exists(rutaApp) ? new FileInfo(rutaApp).LastWriteTimeUtc : null;
            DateTime? fechaUsuario = File.Exists(RutaXmlUsuario) ? new FileInfo(RutaXmlUsuario).LastWriteTimeUtc : null;

            return (
                rutaApp: rutaApp,
                rutaUsuario: RutaXmlUsuario,
                existeApp: File.Exists(rutaApp),
                existeUsuario: File.Exists(RutaXmlUsuario),
                fechaApp: fechaApp,
                fechaUsuario: fechaUsuario
            );
        }
    }
}
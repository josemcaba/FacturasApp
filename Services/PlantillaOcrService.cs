using FacturasApp.Models;
using System.Xml.Serialization;
using System.Reflection;

namespace FacturasApp.Services
{
    public class PlantillaOcrService
    {
        // ── Ubicación en AppData (modificable por usuario - datos persistentes) ──
        private static readonly string RutaDirectorio = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FacturasApp");

        private static readonly string RutaXmlUsuario = Path.Combine(
            RutaDirectorio, "plantillas_ocr.xml");

        // ── Archivo de control de versión ──
        // Almacena qué versión del ensamblado fue la última que instaló las plantillas
        private static readonly string RutaVersionInfo = Path.Combine(
            RutaDirectorio, ".plantillas_version");

        private readonly XmlSerializer _serializer =
            new(typeof(PlantillasOcrColeccion));

        // ── Carga ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Carga las plantillas del usuario. Si no existen, las extrae desde
        /// el ensamblado (recurso embebido) en primer inicio.
        /// En actualizaciones, solo sobrescribe si la versión cambió.
        /// Siempre retorna una PlantillasOcrColeccion (nunca null).
        /// </summary>
        public PlantillasOcrColeccion Cargar()
        {
            try
            {
                // Asegurar que el directorio en AppData existe
                Directory.CreateDirectory(RutaDirectorio);

                // Solo copiar plantillas iniciales o en caso de actualización
                InstalarOActualizarPlantillasSiNecesario();

                // Intentar cargar del directorio de usuario (AppData)
                if (File.Exists(RutaXmlUsuario))
                {
                    try
                    {
                        using var stream = new FileStream(RutaXmlUsuario, FileMode.Open);
                        var resultado = _serializer.Deserialize(stream) as PlantillasOcrColeccion;

                        if (resultado != null)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"✓ Plantillas cargadas desde: {RutaXmlUsuario}");
                            return resultado;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"✗ Error cargando plantillas: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"⚠ Archivo de plantillas no encontrado en: {RutaXmlUsuario}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error en Cargar(): {ex.Message}");
            }

            // Siempre retornar una colección válida, nunca null
            return new PlantillasOcrColeccion();
        }

        // ── Obtener versión del ensamblado ────────────────────────────────────

        /// <summary>
        /// Obtiene la versión actual del ensamblado.
        /// </summary>
        private static string ObtenerVersionEnsamblado()
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return version?.ToString() ?? "0.0.0.0";
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        /// <summary>
        /// Obtiene la versión guardada la última vez que se instalaron las plantillas.
        /// </summary>
        private static string ObtenerVersionGuardada()
        {
            try
            {
                if (File.Exists(RutaVersionInfo))
                {
                    return File.ReadAllText(RutaVersionInfo).Trim();
                }
            }
            catch { }

            return "0.0.0.0";
        }

        /// <summary>
        /// Guarda la versión actual del ensamblado.
        /// </summary>
        private static void GuardarVersionActual()
        {
            try
            {
                string version = ObtenerVersionEnsamblado();
                File.WriteAllText(RutaVersionInfo, version);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error guardando versión: {ex.Message}");
            }
        }

        // ── Obtener plantillas desde recurso embebido ─────────────────────────

        /// <summary>
        /// Obtiene el contenido del archivo plantillas_ocr.xml incrustado en el ensamblado.
        /// Intenta múltiples nombres de recurso posibles.
        /// Retorna null si no encuentra el recurso.
        /// </summary>
        private static string? ObtenerPlantillasDelEnsamblado()
        {
            try
            {
                var ensamblado = Assembly.GetExecutingAssembly();

                // Posibles nombres del recurso (probar varios)
                string[] nombresRecurso = new[]
                {
                    "FacturasApp.Data.plantillas_ocr.xml",
                    "Data.plantillas_ocr.xml",
                    "plantillas_ocr.xml",
                    "FacturasApp.plantillas_ocr.xml"
                };

                foreach (var nombreRecurso in nombresRecurso)
                {
                    try
                    {
                        using var stream = ensamblado.GetManifestResourceStream(nombreRecurso);
                        if (stream != null)
                        {
                            using var reader = new StreamReader(stream);
                            string contenido = reader.ReadToEnd();
                            System.Diagnostics.Debug.WriteLine(
                                $"✓ Plantillas extraídas del ensamblado (recurso: {nombreRecurso}, {contenido.Length} bytes)");
                            return contenido;
                        }
                    }
                    catch { }
                }

                // Si no encontramos nada, listar todos los recursos disponibles para debugging
                System.Diagnostics.Debug.WriteLine("✗ Recurso de plantillas no encontrado. Recursos disponibles:");
                var recursosDisponibles = ensamblado.GetManifestResourceNames();
                foreach (var r in recursosDisponibles)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {r}");
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error obteniendo plantillas del ensamblado: {ex.Message}");
                return null;
            }
        }

        // ── Instalar o actualizar plantillas solo si es necesario ──────────────

        /// <summary>
        /// Instala las plantillas en primer inicio o las actualiza si cambió la versión.
        /// NO modifica las plantillas si el usuario ya las editó en la misma versión.
        /// </summary>
        private void InstalarOActualizarPlantillasSiNecesario()
        {
            try
            {
                string versionActual = ObtenerVersionEnsamblado();
                string versionGuardada = ObtenerVersionGuardada();

                System.Diagnostics.Debug.WriteLine(
                    $"Versión ensamblado: {versionActual}, Versión guardada: {versionGuardada}");

                // Primera vez: no existe archivo del usuario
                if (!File.Exists(RutaXmlUsuario))
                {
                    System.Diagnostics.Debug.WriteLine("📋 Primera instalación: copiando plantillas iniciales...");
                    CopiarPlantillasDelEnsamblado();
                    GuardarVersionActual();
                    return;
                }

                // Si la versión cambió (actualización de la aplicación)
                // Sobrescribir las plantillas con la nueva versión
                if (versionActual != versionGuardada)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"🔄 Actualización detectada ({versionGuardada} → {versionActual}): actualizando plantillas...");
                    CopiarPlantillasDelEnsamblado();
                    GuardarVersionActual();
                    return;
                }

                // Versión no cambió: dejar las plantillas del usuario intactas
                System.Diagnostics.Debug.WriteLine("ℹ Versión sin cambios: manteniendo plantillas del usuario");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error en InstalarOActualizarPlantillas: {ex.Message}");
            }
        }

        /// <summary>
        /// Copia el archivo de plantillas desde el ensamblado (recurso embebido) a AppData.
        /// </summary>
        private void CopiarPlantillasDelEnsamblado()
        {
            try
            {
                // Obtener contenido desde el ensamblado
                string? contenidoPlantillas = ObtenerPlantillasDelEnsamblado();

                if (string.IsNullOrEmpty(contenidoPlantillas))
                {
                    System.Diagnostics.Debug.WriteLine("✗ No se pudo obtener plantillas del ensamblado");
                    return;
                }

                // Escribir o actualizar el contenido en AppData
                File.WriteAllText(RutaXmlUsuario, contenidoPlantillas);
                System.Diagnostics.Debug.WriteLine($"✓ Plantillas instaladas/actualizadas en: {RutaXmlUsuario}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error copiando plantillas: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"✓ Plantillas guardadas en: {RutaXmlUsuario}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error guardando plantillas: {ex.Message}");
            }
        }

        // ── Operaciones sobre plantillas ──────────────────────────────────────

        public PlantillaOcr? ObtenerPorEmisor(string nombreEmisor)
        {
            var coleccion = Cargar();
            return coleccion.Plantillas.FirstOrDefault(p =>
                p.Emisor.Equals(nombreEmisor, StringComparison.OrdinalIgnoreCase));
        }

        public void GuardarPlantilla(PlantillaOcr plantilla)
        {
            var coleccion = Cargar();
            var existente = coleccion.Plantillas.FirstOrDefault(p =>
                p.Emisor.Equals(plantilla.Emisor, StringComparison.OrdinalIgnoreCase));

            if (existente != null)
                coleccion.Plantillas.Remove(existente);

            coleccion.Plantillas.Add(plantilla);
            Guardar(coleccion);
        }

        public void EliminarPlantilla(string nombreEmisor)
        {
            var coleccion = Cargar();
            coleccion.Plantillas.RemoveAll(p =>
                p.Emisor.Equals(nombreEmisor, StringComparison.OrdinalIgnoreCase));
            Guardar(coleccion);
        }

        public List<string> ObtenerEmisoresConPlantilla()
        {
            return Cargar().Plantillas
                .Select(p => p.Emisor)
                .OrderBy(e => e)
                .ToList();
        }

        // ── Diagnóstico ──────────────────────────────────────────────────────

        public (string rutaUsuario, bool existeUsuario, bool recursoDisponible, string versionEnsamblado, string versionGuardada)
            ObtenerInfoRutas()
        {
            string? contenido = ObtenerPlantillasDelEnsamblado();
            bool recursoDisponible = contenido != null;

            return (
                rutaUsuario: RutaXmlUsuario,
                existeUsuario: File.Exists(RutaXmlUsuario),
                recursoDisponible: recursoDisponible,
                versionEnsamblado: ObtenerVersionEnsamblado(),
                versionGuardada: ObtenerVersionGuardada()
            );
        }
    }
}
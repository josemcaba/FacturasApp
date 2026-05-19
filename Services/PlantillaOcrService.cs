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

        private readonly XmlSerializer _serializer =
            new(typeof(PlantillasOcrColeccion));

        // ── Carga ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Carga las plantillas del usuario. Si no existen, las extrae desde
        /// el ensamblado (recurso embebido) en primer inicio.
        /// Siempre retorna una PlantillasOcrColeccion (nunca null).
        /// </summary>
        public PlantillasOcrColeccion Cargar()
        {
            try
            {
                // Asegurar que el directorio en AppData existe
                Directory.CreateDirectory(RutaDirectorio);

                // Copiar plantillas iniciales o actualizadas si es necesario
                CopiarPlantillasIniciales();

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

        // ── Copiar plantillas iniciales o actualizadas ────────────────────────

        /// <summary>
        /// Copia el archivo de plantillas desde el ensamblado (recurso embebido) a AppData.
        /// </summary>
        private void CopiarPlantillasIniciales()
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

                // Si ya existe, solo actualizar si es diferente
                if (File.Exists(RutaXmlUsuario))
                {
                    try
                    {
                        string contenidoExistente = File.ReadAllText(RutaXmlUsuario);
                        if (contenidoExistente == contenidoPlantillas)
                        {
                            System.Diagnostics.Debug.WriteLine("ℹ Plantillas ya están actualizadas");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠ Error al leer plantillas existentes: {ex.Message}");
                    }
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

        public (string rutaUsuario, bool existeUsuario, bool recursoDisponible)
            ObtenerInfoRutas()
        {
            string? contenido = ObtenerPlantillasDelEnsamblado();
            bool recursoDisponible = contenido != null;

            return (
                rutaUsuario: RutaXmlUsuario,
                existeUsuario: File.Exists(RutaXmlUsuario),
                recursoDisponible: recursoDisponible
            );
        }
    }
}
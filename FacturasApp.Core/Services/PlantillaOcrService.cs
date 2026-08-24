using FacturasApp.Core.Models;
using System.Xml.Serialization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace FacturasApp.Core.Services
{
    public class PlantillaOcrService
    {
        // ── Ubicación en AppData (modificable por usuario - datos persistentes) ──
        private static readonly string RutaDirectorio = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FacturasApp");

        private static readonly string RutaXmlUsuario = Path.Combine(
            RutaDirectorio, "plantillas_ocr.xml");

        // ── Archivo de control: guarda el hash de la última plantilla distribuida ──
        private static readonly string RutaHashControl = Path.Combine(
            RutaDirectorio, ".plantillas_hash");

        private readonly XmlSerializer _serializer =
            new(typeof(PlantillasOcrColeccion));

        // ── Carga ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Carga las plantillas del usuario.
        /// - Primera vez: copia desde el ensamblado
        /// - Si el usuario editó: mantiene sus cambios
        /// - Si hay nueva versión distribuida: sobrescribe con la nueva
        /// Siempre retorna una PlantillasOcrColeccion (nunca null).
        /// </summary>
        public PlantillasOcrColeccion Cargar()
        {
            try
            {
                // Asegurar que el directorio en AppData existe
                Directory.CreateDirectory(RutaDirectorio);

                // Instalar o actualizar plantillas si es necesario
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

        // ── Calcular hash de contenido ───────────────────────────────────────

        /// <summary>
        /// Calcula el hash SHA256 de un contenido de texto.
        /// </summary>
        private static string CalcularHash(string contenido)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(contenido));
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Obtiene el hash guardado de la última plantilla distribuida.
        /// </summary>
        private static string ObtenerHashGuardado()
        {
            try
            {
                if (File.Exists(RutaHashControl))
                {
                    return File.ReadAllText(RutaHashControl).Trim();
                }
            }
            catch { }

            return string.Empty;
        }

        /// <summary>
        /// Guarda el hash de la plantilla actual.
        /// </summary>
        private static void GuardarHash(string contenido)
        {
            try
            {
                string hash = CalcularHash(contenido);
                File.WriteAllText(RutaHashControl, hash);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error guardando hash: {ex.Message}");
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
                    "FacturasApp.Core.Data.plantillas_ocr.xml",
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

        // ── Instalar o actualizar plantillas ───────────────────────────────────

        /// <summary>
        /// Instala las plantillas en primer inicio.
        /// Si el usuario las editó, mantiene sus cambios.
        /// Si hay nueva versión (distinto hash), sobrescribe.
        /// </summary>
        private void InstalarOActualizarPlantillasSiNecesario()
        {
            try
            {
                // Obtener plantillas del ensamblado
                string? contenidoNuevo = ObtenerPlantillasDelEnsamblado();

                if (string.IsNullOrEmpty(contenidoNuevo))
                {
                    System.Diagnostics.Debug.WriteLine("✗ No se pudo obtener plantillas del ensamblado");
                    return;
                }

                string hashNuevo = CalcularHash(contenidoNuevo);
                string hashGuardado = ObtenerHashGuardado();

                System.Diagnostics.Debug.WriteLine(
                    $"Hash actual: {hashNuevo.Substring(0, 8)}..., Hash guardado: {hashGuardado.Substring(0, Math.Min(8, hashGuardado.Length))}...");

                // Primera vez: no existe archivo del usuario
                if (!File.Exists(RutaXmlUsuario))
                {
                    System.Diagnostics.Debug.WriteLine("📋 Primera instalación: instalando plantillas iniciales...");
                    File.WriteAllText(RutaXmlUsuario, contenidoNuevo);
                    GuardarHash(contenidoNuevo);
                    System.Diagnostics.Debug.WriteLine($"✓ Plantillas instaladas en: {RutaXmlUsuario}");
                    return;
                }

                // Si el hash cambió, significa que hay nueva versión distribuida
                if (hashNuevo != hashGuardado)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"🔄 Nueva versión detectada: actualizando plantillas...");
                    File.WriteAllText(RutaXmlUsuario, contenidoNuevo);
                    GuardarHash(contenidoNuevo);
                    System.Diagnostics.Debug.WriteLine($"✓ Plantillas actualizadas en: {RutaXmlUsuario}");
                    return;
                }

                // Hash no cambió: usuario puede haber editado, mantener sus cambios
                System.Diagnostics.Debug.WriteLine("ℹ Versión sin cambios: manteniendo plantillas del usuario");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error en InstalarOActualizarPlantillas: {ex.Message}");
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

        public (string rutaUsuario, bool existeUsuario, bool recursoDisponible, string hashActual, string hashGuardado)
            ObtenerInfoRutas()
        {
            string? contenido = ObtenerPlantillasDelEnsamblado();
            bool recursoDisponible = contenido != null;
            string hashActual = contenido != null ? CalcularHash(contenido).Substring(0, 8) : "N/A";
            string hashGuardado = ObtenerHashGuardado().Substring(0, Math.Min(8, ObtenerHashGuardado().Length));

            return (
                rutaUsuario: RutaXmlUsuario,
                existeUsuario: File.Exists(RutaXmlUsuario),
                recursoDisponible: recursoDisponible,
                hashActual: hashActual,
                hashGuardado: hashGuardado
            );
        }
    }
}
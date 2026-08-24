using System.Reflection;

namespace FacturasApp.Core.Services
{
    /// <summary>
    /// Servicio para detectar si la aplicación se ejecuta en:
    /// - DESARROLLO: Desde Visual Studio o carpeta bin/Debug
    /// - PRODUCCIÓN: Instalada en Program Files o similar (cliente)
    /// </summary>
    public class EnvironmentService
    {
        /// <summary>
        /// Detecta si estamos en modo desarrollo.
        /// - True: Si se ejecuta desde Visual Studio o bin/Debug
        /// - False: Si se ejecuta desde una ubicación de producción (Program Files, etc.)
        /// </summary>
        public static bool EsDesarrollo()
        {
            try
            {
                var rutaEjecucion = Assembly.GetExecutingAssembly().Location;

                // Indicadores de desarrollo
                bool esDebugFolder = rutaEjecucion.Contains("bin\\Debug", StringComparison.OrdinalIgnoreCase) ||
                                    rutaEjecucion.Contains("bin/Debug", StringComparison.OrdinalIgnoreCase);

                bool esVsFolder = rutaEjecucion.Contains("\\FacturasApp\\", StringComparison.OrdinalIgnoreCase);

                System.Diagnostics.Debug.WriteLine(
                    $"📍 Entorno detectado: {(esDebugFolder || esVsFolder ? "DESARROLLO" : "PRODUCCIÓN")}\n" +
                    $"   Ruta: {rutaEjecucion}");

                return esDebugFolder || esVsFolder;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error detectando entorno: {ex.Message}");
                // Si hay error, asumir que es producción (más seguro)
                return false;
            }
        }

        /// <summary>
        /// Obtiene información de depuración sobre el entorno.
        /// </summary>
        public static (string rutaEjecucion, bool esDesarrollo) ObtenerInfoEntorno()
        {
            try
            {
                var rutaEjecucion = Assembly.GetExecutingAssembly().Location;
                return (rutaEjecucion, EsDesarrollo());
            }
            catch
            {
                return ("Desconocida", false);
            }
        }
    }
}
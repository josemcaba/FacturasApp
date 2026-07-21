using FacturasApp.Models;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace FacturasApp.Services
{
    public class EmisorService
    {
        // ── Ubicación en AppData (datos persistentes del usuario) ──
        private static readonly string RutaDirectorio = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FacturasApp");

        private static readonly string RutaXmlUsuario = Path.Combine(
            RutaDirectorio, "emisores.xml");

        private static readonly string RutaHashControl = Path.Combine(
            RutaDirectorio, ".emisores_hash");

        private readonly XmlSerializer _serializer = new(typeof(EmisorCollection));

        // ── Carga ─────────────────────────────────────────────────────────────

        public EmisorCollection Cargar()
        {
            try
            {
                Directory.CreateDirectory(RutaDirectorio);
                InstalarOActualizarSiNecesario();

                if (File.Exists(RutaXmlUsuario))
                {
                    try
                    {
                        using var stream = new FileStream(RutaXmlUsuario, FileMode.Open);
                        var resultado = _serializer.Deserialize(stream) as EmisorCollection;

                        if (resultado != null)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"✓ Emisores cargados: {resultado.Emisores.Count} desde {RutaXmlUsuario}");
                            return resultado;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"✗ Error cargando emisores: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error en EmisorService.Cargar(): {ex.Message}");
            }

            return new EmisorCollection();
        }

        // ── Guardar ────────────────────────────────────────────────────────────

        public void Guardar(EmisorCollection coleccion)
        {
            try
            {
                Directory.CreateDirectory(RutaDirectorio);

                // Crear backup antes de guardar
                CrearBackup();

                using var stream = new FileStream(RutaXmlUsuario,
                    FileMode.Create, FileAccess.Write);
                _serializer.Serialize(stream, coleccion);

                System.Diagnostics.Debug.WriteLine(
                    $"✓ Emisores guardados: {coleccion.Emisores.Count} en {RutaXmlUsuario}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error guardando emisores: {ex.Message}");
            }
        }

        // ── CRUD por NIF (clave única) ──────────────────────────────────────────

        public EmisorDefinicion? ObtenerPorNif(string nif)
        {
            var coleccion = Cargar();
            return coleccion.Emisores.FirstOrDefault(e =>
                e.Nif.Equals(nif, StringComparison.OrdinalIgnoreCase));
        }

        public EmisorDefinicion? ObtenerPorNombre(string nombre)
        {
            var coleccion = Cargar();
            return coleccion.Emisores.FirstOrDefault(e =>
                e.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
        }

        public bool ExisteNif(string nif)
        {
            var coleccion = Cargar();
            return coleccion.Emisores.Any(e =>
                e.Nif.Equals(nif, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Guarda un emisor. Si ya existe un emisor con el mismo NIF, lo reemplaza.
        /// Retorna true si se guardó, false si el NIF es inválido o vacío.
        /// </summary>
        public bool GuardarEmisor(EmisorDefinicion emisor)
        {
            if (string.IsNullOrWhiteSpace(emisor.Nif))
                return false;

            var coleccion = Cargar();
            var existente = coleccion.Emisores.FirstOrDefault(e =>
                e.Nif.Equals(emisor.Nif, StringComparison.OrdinalIgnoreCase));

            if (existente != null)
                coleccion.Emisores.Remove(existente);

            coleccion.Emisores.Add(emisor);
            Guardar(coleccion);
            return true;
        }

        /// <summary>
        /// Elimina un emisor por NIF. Retorna true si se eliminó.
        /// </summary>
        public bool EliminarPorNif(string nif)
        {
            var coleccion = Cargar();
            int eliminados = coleccion.Emisores.RemoveAll(e =>
                e.Nif.Equals(nif, StringComparison.OrdinalIgnoreCase));

            if (eliminados > 0)
                Guardar(coleccion);

            return eliminados > 0;
        }

        public List<EmisorDefinicion> ObtenerTodos()
        {
            return Cargar().Emisores
                .OrderBy(e => e.Nombre)
                .ToList();
        }

        public List<string> ObtenerNifs()
        {
            return Cargar().Emisores
                .Select(e => e.Nif)
                .OrderBy(n => n)
                .ToList();
        }

        /// <summary>
        /// Fuerza la recarga de emisores.xml desde el recurso embebido del ensamblado.
        /// Útil cuando se ha actualizado el embebido pero el hash no cambió.
        /// </summary>
        public int ForzarActualizacionDesdeEnsamblado()
        {
            try
            {
                string? contenido = ObtenerEmisoresDelEnsamblado();
                if (string.IsNullOrEmpty(contenido)) return 0;

                File.WriteAllText(RutaXmlUsuario, contenido);
                GuardarHash(contenido);

                var coleccion = Cargar();
                System.Diagnostics.Debug.WriteLine(
                    $"✓ Forzado actualización: {coleccion.Emisores.Count} emisores desde ensamblado");
                return coleccion.Emisores.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error forzando actualización: {ex.Message}");
                return 0;
            }
        }

        // ── Importación con validación de duplicados ────────────────────────────

        /// <summary>
        /// Importa una lista de emisores. Si un NIF ya existe, lo omite.
        /// Retorna (importados, omitidos_por_duplicado).
        /// </summary>
        public (int importados, int duplicados) Importar(List<EmisorDefinicion> emisores)
        {
            var coleccion = Cargar();
            int importados = 0;
            int duplicados = 0;

            foreach (var emisor in emisores)
            {
                if (string.IsNullOrWhiteSpace(emisor.Nif))
                    continue;

                var existente = coleccion.Emisores.FirstOrDefault(e =>
                    e.Nif.Equals(emisor.Nif, StringComparison.OrdinalIgnoreCase));

                if (existente != null)
                {
                    duplicados++;
                    continue;
                }

                coleccion.Emisores.Add(emisor);
                importados++;
            }

            if (importados > 0)
                Guardar(coleccion);

            return (importados, duplicados);
        }

        // ── Backup ──────────────────────────────────────────────────────────────

        private void CrearBackup()
        {
            try
            {
                if (!File.Exists(RutaXmlUsuario)) return;

                string rutaBackup = Path.Combine(RutaDirectorio,
                    $"emisores_{DateTime.Now:yyyyMMdd_HHmmss}.xml.bak");

                File.Copy(RutaXmlUsuario, rutaBackup, true);

                // Mantener solo los últimos 5 backups
                var backups = Directory.GetFiles(RutaDirectorio, "emisores_*.xml.bak")
                    .OrderByDescending(f => f)
                    .Skip(5)
                    .ToList();

                foreach (var backup in backups)
                    File.Delete(backup);
            }
            catch { }
        }

        // ── Hash y sincronización con ensamblado ───────────────────────────────

        private static string CalcularHash(string contenido)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(contenido));
            return Convert.ToBase64String(hash);
        }

        private static string ObtenerHashGuardado()
        {
            try
            {
                if (File.Exists(RutaHashControl))
                    return File.ReadAllText(RutaHashControl).Trim();
            }
            catch { }
            return string.Empty;
        }

        private static void GuardarHash(string contenido)
        {
            try
            {
                string hash = CalcularHash(contenido);
                File.WriteAllText(RutaHashControl, hash);
            }
            catch { }
        }

        private static string? ObtenerEmisoresDelEnsamblado()
        {
            try
            {
                var ensamblado = Assembly.GetExecutingAssembly();

                string[] nombresRecurso =
                [
                    "FacturasApp.Data.emisores.xml",
                    "Data.emisores.xml",
                    "emisores.xml"
                ];

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
                                $"✓ Emisores del ensamblado (recurso: {nombreRecurso}, {contenido.Length} bytes)");
                            return contenido;
                        }
                    }
                    catch { }
                }

                System.Diagnostics.Debug.WriteLine("✗ Recurso emisores.xml no encontrado en ensamblado");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error obteniendo emisores del ensamblado: {ex.Message}");
                return null;
            }
        }

        private void InstalarOActualizarSiNecesario()
        {
            try
            {
                string? contenidoNuevo = ObtenerEmisoresDelEnsamblado();
                if (string.IsNullOrEmpty(contenidoNuevo)) return;

                string hashNuevo = CalcularHash(contenidoNuevo);
                string hashGuardado = ObtenerHashGuardado();

                bool necesitaActualizar = false;

                if (!File.Exists(RutaXmlUsuario))
                {
                    System.Diagnostics.Debug.WriteLine("📋 Primera instalación: emisores.xml inicial...");
                    necesitaActualizar = true;
                }
                else if (hashNuevo != hashGuardado)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "🔄 Nueva versión detectada (hash distinto): actualizando emisores.xml...");
                    necesitaActualizar = true;
                }
                else
                {
                    // Verificar también que el archivo del usuario tenga todos los emisores
                    // del embebido (por si el usuario editó manualmente o hay corrupción)
                    try
                    {
                        using var streamUsuario = new FileStream(RutaXmlUsuario, FileMode.Open);
                        var usuario = _serializer.Deserialize(streamUsuario) as EmisorCollection;
                        using var streamEmbebido = new System.IO.StringReader(contenidoNuevo);
                        var embebido = _serializer.Deserialize(streamEmbebido) as EmisorCollection;

                        if (usuario != null && embebido != null &&
                            usuario.Emisores.Count < embebido.Emisores.Count)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"🔄 Emisores insuficientes en usuario ({usuario.Emisores.Count}" +
                                $" < {embebido.Emisores.Count}): actualizando...");
                            necesitaActualizar = true;
                        }
                    }
                    catch
                    {
                        // Si hay error parseando el usuario, forzar actualización
                        necesitaActualizar = true;
                    }
                }

                if (necesitaActualizar)
                {
                    File.WriteAllText(RutaXmlUsuario, contenidoNuevo);
                    GuardarHash(contenidoNuevo);
                    System.Diagnostics.Debug.WriteLine(
                        $"✓ emisores.xml actualizado en {RutaXmlUsuario}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ℹ Sin cambios: manteniendo emisores del usuario");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error en InstalarOActualizarSiNecesario: {ex.Message}");
            }
        }

        // ── Diagnóstico ──────────────────────────────────────────────────────

        public (string rutaUsuario, bool existeUsuario, bool recursoDisponible,
                string hashActual, string hashGuardado) ObtenerInfoRutas()
        {
            string? contenido = ObtenerEmisoresDelEnsamblado();
            bool recursoDisponible = contenido != null;
            string hashActual = contenido != null ? CalcularHash(contenido).Substring(0, 8) : "N/A";
            string hashGuardado = ObtenerHashGuardado();
            hashGuardado = hashGuardado.Length > 8 ? hashGuardado.Substring(0, 8) : hashGuardado;

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

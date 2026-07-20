using FacturasApp.Models;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace FacturasApp.Services
{
    public class ProveedorConfigService
    {
        private static readonly string RutaDirectorio = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FacturasApp");

        private static readonly string RutaXmlUsuario = Path.Combine(
            RutaDirectorio, "proveedores_config.xml");

        private static readonly string RutaHashControl = Path.Combine(
            RutaDirectorio, ".proveedores_hash");

        private static readonly string NombreRecurso = "FacturasApp.Data.proveedores_config.xml";

        private readonly XmlSerializer _serializer = new(typeof(ProveedoresConfiguracion));

        public ProveedoresConfiguracion Cargar()
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
                        var resultado = _serializer.Deserialize(stream) as ProveedoresConfiguracion;
                        if (resultado != null)
                            return resultado;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error cargando proveedores_config.xml: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en Cargar(): {ex.Message}");
            }

            return new ProveedoresConfiguracion();
        }

        public void Guardar(ProveedoresConfiguracion config)
        {
            try
            {
                Directory.CreateDirectory(RutaDirectorio);
                using var stream = new FileStream(RutaXmlUsuario, FileMode.Create, FileAccess.Write);
                _serializer.Serialize(stream, config);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando proveedores_config.xml: {ex.Message}");
            }
        }

        public ProveedorConfig? ObtenerPorNombre(string nombre)
        {
            return Cargar().Proveedores.FirstOrDefault(p =>
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
        }

        public ProveedorConfig? ObtenerPorIdentificadores(string texto)
        {
            return Cargar().Proveedores.FirstOrDefault(p =>
                p.Identificadores.Count > 0 &&
                p.Identificadores.All(id =>
                    texto.Contains(id, StringComparison.OrdinalIgnoreCase)));
        }

        public List<string> ObtenerNombresProveedores()
        {
            return Cargar().Proveedores
                .Select(p => p.Nombre)
                .OrderBy(n => n)
                .ToList();
        }

        public void GuardarProveedor(ProveedorConfig proveedor)
        {
            var config = Cargar();
            var existente = config.Proveedores.FirstOrDefault(p =>
                p.Nombre.Equals(proveedor.Nombre, StringComparison.OrdinalIgnoreCase));
            if (existente != null)
                config.Proveedores.Remove(existente);
            config.Proveedores.Add(proveedor);
            Guardar(config);
        }

        public void EliminarProveedor(string nombre)
        {
            var config = Cargar();
            config.Proveedores.RemoveAll(p =>
                p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
            Guardar(config);
        }

        private void InstalarOActualizarSiNecesario()
        {
            try
            {
                string? contenidoNuevo = ObtenerDelEnsamblado();
                if (string.IsNullOrEmpty(contenidoNuevo))
                    return;

                string hashNuevo = CalcularHash(contenidoNuevo);
                string hashGuardado = ObtenerHashGuardado();

                if (!File.Exists(RutaXmlUsuario))
                {
                    File.WriteAllText(RutaXmlUsuario, contenidoNuevo);
                    GuardarHash(contenidoNuevo);
                    return;
                }

                if (hashNuevo != hashGuardado)
                {
                    File.WriteAllText(RutaXmlUsuario, contenidoNuevo);
                    GuardarHash(contenidoNuevo);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en InstalarOActualizar: {ex.Message}");
            }
        }

        private static string? ObtenerDelEnsamblado()
        {
            try
            {
                var ensamblado = Assembly.GetExecutingAssembly();
                using var stream = ensamblado.GetManifestResourceStream(NombreRecurso);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo recurso embebido: {ex.Message}");
            }
            return null;
        }

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando hash: {ex.Message}");
            }
        }
    }
}

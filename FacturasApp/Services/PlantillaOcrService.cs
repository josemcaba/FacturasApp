using FacturasApp.Models;
using System.Xml.Serialization;

namespace FacturasApp.Services
{
    public class PlantillaOcrService
    {
        private static readonly string RutaDirectorio = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "FacturasApp");

        private static readonly string RutaXml = Path.Combine(
            RutaDirectorio, "plantillas_ocr.xml");

        private readonly XmlSerializer _serializer =
            new(typeof(PlantillasOcrColeccion));

        // ── Carga ─────────────────────────────────────────────────────────────

        public PlantillasOcrColeccion Cargar()
        {
            if (!File.Exists(RutaXml))
                return new PlantillasOcrColeccion();

            try
            {
                using var stream = new FileStream(RutaXml, FileMode.Open);
                return _serializer.Deserialize(stream)
                       as PlantillasOcrColeccion
                       ?? new PlantillasOcrColeccion();
            }
            catch
            {
                return new PlantillasOcrColeccion();
            }
        }

        // ── Guarda ────────────────────────────────────────────────────────────

        public void Guardar(PlantillasOcrColeccion coleccion)
        {
            Directory.CreateDirectory(RutaDirectorio);

            using var stream = new FileStream(RutaXml,
                FileMode.Create, FileAccess.Write);

            _serializer.Serialize(stream, coleccion);
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
    }
}
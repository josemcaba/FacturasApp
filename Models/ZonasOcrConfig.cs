using System.Xml.Serialization;

namespace FacturasApp.Models
{
    public class ZonasOcrConfig
    {
        [XmlElement("AnchoReferencia")]
        public double AnchoReferencia { get; set; } = 1000;

        [XmlElement("AltoReferencia")]
        public double AltoReferencia { get; set; } = 1000;

        [XmlElement("TipoCoordenada")]
        public string TipoCoordenada { get; set; } = "Normalizadas";

        [XmlArray("Zonas")]
        [XmlArrayItem("Zona")]
        public List<ZonaOcrDefinicion> Zonas { get; set; } = new();

        /// <summary>
        /// Convierte la configuración del XML al modelo PlantillaOcr existente.
        /// </summary>
        public PlantillaOcr APlantillaOcr(string nombreEmisor)
        {
            var plantilla = new PlantillaOcr
            {
                Emisor = nombreEmisor,
                AnchoReferencia = AnchoReferencia,
                AltoReferencia = AltoReferencia,
                TipoCoordenada = TipoCoordenada == "AbsolutasPuntos"
                    ? TipoCoordenadas.AbsolutasPuntos
                    : TipoCoordenadas.Normalizadas
            };

            foreach (var zona in Zonas)
            {
                plantilla.Zonas.Add(new ZonaOcr
                {
                    Campo = zona.Campo,
                    NumPagina = zona.Pagina,
                    X = zona.X,
                    Y = zona.Y,
                    Ancho = zona.Ancho,
                    Alto = zona.Alto,
                    RegexPersonalizada = zona.Regex ?? string.Empty,
                    RegexRespaldo = zona.RegexRespaldo,
                    Opcional = zona.Opcional,
                    Preprocesamiento = new PreprocesamientoOcr
                    {
                        EscalaGrises = zona.Preprocesamiento?.EscalaGrises ?? true,
                        EliminarRuido = zona.Preprocesamiento?.EliminarRuido ?? true,
                        Binarizacion = zona.Preprocesamiento?.Binarizacion ?? false,
                        UmbralBinarizacion = zona.Preprocesamiento?.UmbralBinarizacion ?? 128,
                        Redimensionar = zona.Preprocesamiento?.Redimensionar ?? false,
                        FactorEscala = zona.Preprocesamiento?.FactorEscala ?? 2,
                        InvertirColores = zona.Preprocesamiento?.InvertirColores ?? false,
                        AutoRecortar = zona.Preprocesamiento?.AutoRecortar ?? false,
                        PaddingAutoRecorte = zona.Preprocesamiento?.PaddingAutoRecorte ?? 5
                    }
                });
            }

            return plantilla;
        }
    }

    public class ZonaOcrDefinicion
    {
        [XmlAttribute("Campo")]
        public string Campo { get; set; } = string.Empty;

        [XmlAttribute("Pagina")]
        public int Pagina { get; set; } = 1;

        [XmlAttribute("X")]
        public double X { get; set; }

        [XmlAttribute("Y")]
        public double Y { get; set; }

        [XmlAttribute("Ancho")]
        public double Ancho { get; set; }

        [XmlAttribute("Alto")]
        public double Alto { get; set; }

        [XmlAttribute("Regex")]
        public string? Regex { get; set; }

        [XmlElement("RegexRespaldo")]
        public string? RegexRespaldo { get; set; }

        [XmlElement("Opcional")]
        public bool Opcional { get; set; } = false;

        [XmlElement("Preprocesamiento")]
        public PreprocesamientoOcrConfig? Preprocesamiento { get; set; }
    }

    public class PreprocesamientoOcrConfig
    {
        [XmlElement("EscalaGrises")]
        public bool EscalaGrises { get; set; } = true;

        [XmlElement("Binarizacion")]
        public bool Binarizacion { get; set; } = false;

        [XmlElement("UmbralBinarizacion")]
        public int UmbralBinarizacion { get; set; } = 128;

        [XmlElement("Redimensionar")]
        public bool Redimensionar { get; set; } = false;

        [XmlElement("FactorEscala")]
        public int FactorEscala { get; set; } = 2;

        [XmlElement("EliminarRuido")]
        public bool EliminarRuido { get; set; } = true;

        [XmlElement("InvertirColores")]
        public bool InvertirColores { get; set; } = false;

        [XmlElement("AutoRecortar")]
        public bool AutoRecortar { get; set; } = false;

        [XmlElement("PaddingAutoRecorte")]
        public int PaddingAutoRecorte { get; set; } = 5;
    }
}

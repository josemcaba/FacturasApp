namespace FacturasApp.Core.Services
{
    /// <summary>
    /// Formatos de documento que admite la aplicación para facturas.
    /// Sólo extensiones (sin dependencias de dibujo): Core no necesita saber
    /// cargar la imagen, sólo decidir qué rama del pipeline usar y qué
    /// ofrecer en los diálogos de selección.
    /// </summary>
    public static class FormatosDocumento
    {
        /// <summary>Extensiones de imagen admitidas (GDI+ las decodifica de serie).</summary>
        public static readonly IReadOnlyList<string> ExtensionesImagen = new[]
        {
            ".jpg", ".jpeg", ".png"
        };

        /// <summary>Extensiones de factura: PDF + imágenes.</summary>
        public static readonly IReadOnlyList<string> ExtensionesFactura = new[]
        {
            ".pdf"
        }.Concat(ExtensionesImagen).ToList();

        /// <summary>
        /// Filtro de OpenFileDialog para facturas: PDF + imágenes admitidas
        /// (JPG/PNG), con subfiltros por formato. Texto genérico para que
        /// cada formulario lo use tal cual.
        /// </summary>
        public static string FiltroDialogo
        {
            get
            {
                string imagenes = string.Join(";", ExtensionesImagen
                    .Select(e => "*" + e));
                return $"Facturas (PDF e imágenes)|*.pdf;{imagenes}"
                     + $"|PDF (*.pdf)|*.pdf"
                     + $"|Imágenes ({imagenes})|{imagenes}";
            }
        }

        /// <summary>¿La ruta es un PDF?</summary>
        public static bool EsPdf(string ruta) =>
            !string.IsNullOrWhiteSpace(ruta)
            && string.Equals(Path.GetExtension(ruta), ".pdf", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// ¿La ruta es una imagen admitida (JPG/PNG)? Las imágenes no tienen
        /// capa de texto: van directamente por OCR.
        /// </summary>
        public static bool EsImagen(string ruta) =>
            !string.IsNullOrWhiteSpace(ruta)
            && ExtensionesImagen.Contains(Path.GetExtension(ruta),
                StringComparer.OrdinalIgnoreCase);

        /// <summary>¿La ruta es un documento que la app sabe procesar (PDF o imagen)?</summary>
        public static bool EsFactura(string ruta) =>
            EsPdf(ruta) || EsImagen(ruta);
    }
}

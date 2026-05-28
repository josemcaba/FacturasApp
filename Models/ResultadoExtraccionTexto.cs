namespace FacturasApp.Models
{
    /// <summary>
    /// Encapsula el resultado de una extracción de texto,
    /// incluyendo el texto y el método utilizado.
    /// </summary>
    public class ResultadoExtraccionTexto
    {
        /// <summary>
        /// El texto extraído de la zona.
        /// </summary>
        public string Texto { get; set; } = string.Empty;

        /// <summary>
        /// Método de extracción utilizado.
        /// </summary>
        public MetodoExtraccion Metodo { get; set; }

        /// <summary>
        /// Si es true, significa que la zona no tenía texto (vacía).
        /// </summary>
        public bool EstaVacia { get; set; }

        public enum MetodoExtraccion
        {
            /// <summary>
            /// Texto extraído directamente del PDF (sin procesar imagen)
            /// </summary>
            TextoSeleccionable,

            /// <summary>
            /// Texto extraído usando OCR sobre la imagen del PDF
            /// </summary>
            Ocr
        }

        public ResultadoExtraccionTexto(string texto, MetodoExtraccion metodo, bool estaVacia = false)
        {
            Texto = texto;
            Metodo = metodo;
            EstaVacia = estaVacia;
        }

        /// <summary>
        /// Obtiene un prefijo visual para mostrar el método utilizado.
        /// </summary>
        public string ObtenerPrefijo() => Metodo switch
        {
            MetodoExtraccion.TextoSeleccionable => "📋",
            MetodoExtraccion.Ocr => "🔤",
            _ => "❓"
        };

        /// <summary>
        /// Obtiene una descripción del método utilizado.
        /// </summary>
        public string ObtenerDescripcion() => Metodo switch
        {
            MetodoExtraccion.TextoSeleccionable => "Texto directo",
            MetodoExtraccion.Ocr => "OCR",
            _ => "Desconocido"
        };

        /// <summary>
        /// Obtiene el color para mostrar el indicador.
        /// </summary>
        public Color ObtenerColor() => Metodo switch
        {
            MetodoExtraccion.TextoSeleccionable => Color.Green,
            MetodoExtraccion.Ocr => Color.Orange,
            _ => Color.Gray
        };
    }
}

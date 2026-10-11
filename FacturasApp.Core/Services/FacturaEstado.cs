using FacturasApp.Core.Models;

namespace FacturasApp.Core.Services
{
    internal static class FacturaEstado
    {
        // La caché de emisores es estática y compartida con los parsers: esta
        // instancia sólo ahorra reconstruir el XmlSerializer en cada factura.
        private static readonly ConfiguracionEmisores Config = new();

        internal static EstadoFactura Determinar(Factura f)
        {
            // 1. Verificación del total — si no coincide → Error
            if (!f.TotalesCoinciden)
            {
                f.MensajeError.Add("Totales extraido y calculado no coinciden");
                return EstadoFactura.Error;
            }

            // 2. Validación de BaseImponible !0
            if (f.BaseImponible == 0)
            {
                f.MensajeError.Add("Base imponible igual a cero");
                return EstadoFactura.Revisar;
            }

            // 3. NIF del emisor — sólo supera la factura si su emisor está
            // verificado (NifEstado = "Valido" en el XML del emisor, registrado
            // con el botón «Verificar NIF»). Si el NIF no corresponde a ningún
            // emisor dado de alta (p. ej. el parser genérico, que mete en el NIF
            // del emisor un NIF extraído del texto del PDF), no se comprueba.
            var emisor = Config.ObtenerPorNif(f.Emisor.NIF);
            if (emisor != null && emisor.NifEstado != "Valido")
            {
                f.MensajeError.Add(emisor.NifEstado switch
                {
                    "Invalido" => "NIF del Emisor verificado como no válido",
                    "NoDisponible" => "NIF del Emisor sin verificar (VIES no disponible)",
                    _ => "NIF del Emisor sin verificar"
                });
                return EstadoFactura.Error;
            }

            // 4. Validación del NIF del receptor — si no es válido → Error.
            if (!NifValidator.ValidarNif(f.Receptor.NIF))
            {
                f.MensajeError.Add("NIF del Cliente no válido");
                return EstadoFactura.Error;
            }

            // 5.Campos obligatorios — si falta alguno → RevisiónManual. El NIF
            // del emisor tampoco se exige: su verificación es la del paso 3.
            bool camposObligatoriosOk =
                !string.IsNullOrEmpty(f.NumeroFactura) &&
                f.Fecha.HasValue &&
                !string.IsNullOrEmpty(f.Emisor.Nombre) &&
                !string.IsNullOrEmpty(f.Receptor.Nombre) &&
                !string.IsNullOrEmpty(f.Receptor.NIF) &&
                f.ImporteVerificacion != 0.0m;

            if (!camposObligatoriosOk)
            {
                f.MensajeError.Add("Falta uno o más campos obligatorios");
                return EstadoFactura.Revisar;
            }

            // Nombre del cliente (receptor) muy largo — si >40 caracteres → RevisiónManual
            if (f.Receptor.Nombre.Length > 40)
            {
                // Preserve caller intent: limit receptor name to 40 characters safely
                f.Receptor.Nombre = f.Receptor.Nombre.Substring(0, 40);

                f.MensajeError.Add("Nombre del cliente truncado a 40 caracteres");
                return EstadoFactura.Revisar;
            }
            return EstadoFactura.OK;
        }
    }
}

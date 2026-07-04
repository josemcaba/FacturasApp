using FacturasApp.Models;

namespace FacturasApp.Services
{
    internal static class FacturaEstado
    {
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

            // 3. Validación de NIFs — si no son válidos → Error
            if (!NifValidator.ValidarNif(f.Emisor.NIF))
            {
                f.MensajeError.Add("NIF del Emisor no válido");
                return EstadoFactura.Error;
            }
            if (!NifValidator.ValidarNif(f.Receptor.NIF))
            {
                f.MensajeError.Add("NIF del Cliente no válido");
                return EstadoFactura.Error;
            }

            // 4.Campos obligatorios — si falta alguno → RevisiónManual
            bool camposObligatoriosOk =
                !string.IsNullOrEmpty(f.NumeroFactura) &&
                f.Fecha.HasValue &&
                !string.IsNullOrEmpty(f.Emisor.Nombre) &&
                !string.IsNullOrEmpty(f.Emisor.NIF) &&
                !string.IsNullOrEmpty(f.Receptor.Nombre) &&
                !string.IsNullOrEmpty(f.Receptor.NIF) &&
                f.Total != 0.0m;

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

using FacturasApp.Core.Models;

namespace FacturasApp.Core.Services
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

            // 3. Validación del NIF del receptor — si no es válido → Error.
            // El NIF del emisor NO se valida aquí: se comprueba al dar de alta el
            // emisor (español por formato, comunitario contra VIES), de modo que
            // todas sus facturas heredan un emisor ya validado.
            if (!NifValidator.ValidarNif(f.Receptor.NIF))
            {
                f.MensajeError.Add("NIF del Cliente no válido");
                return EstadoFactura.Error;
            }

            // 4.Campos obligatorios — si falta alguno → RevisiónManual. El NIF
            // del emisor tampoco se exige: al igual que su formato, forma parte
            // de la validación del alta.
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

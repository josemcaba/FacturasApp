using FacturasApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturasApp.Services
{
    public enum _Estado
    {
        Pendiente,
        OK,
        Revisar,
        Duplicada,
        Error
    }
    internal class FacturaEstado
    {
        internal static _Estado Determinar(Factura f)
        {
            // 1. Verificación del total — si no coincide → Error
            if (!f.TotalesCoinciden)
            {
                f.MensajeError.Add("Totales extraido y calculado no coinciden");
                return _Estado.Error;
            }

            // 2. Validación de BaseImponible !0
            if (f.BaseImponible == 0)
            {
                f.MensajeError.Add("Base imponible igual a cero");
                return _Estado.Revisar;
            }

            // 3. Validación de NIFs — si no son válidos → Error
            if (!NifValidator.ValidarNif(f.Emisor.NIF))
            {
                f.MensajeError.Add("NIF del Emisor no válido");
                return _Estado.Error;
            }
            if (!NifValidator.ValidarNif(f.Receptor.NIF))
            {
                f.MensajeError.Add("NIF del Cliente no válido");
                return _Estado.Error;
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
                return _Estado.Revisar;
            }

            // Nombre del cliente (receptor) muy largo — si >40 caracteres → RevisiónManual
            if (f.Receptor.Nombre.Length > 40)
            {
                f.MensajeError.Add("Nombre del cliente demasiado largo (>40)");
                return _Estado.Revisar;
            }
            return _Estado.OK;
        }
    }
}

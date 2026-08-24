using FacturasApp.Core.Models;

namespace FacturasApp.Core.Services.Parsers
{
    public interface IInvoiceParser
    {
        string Nombre { get; }
        bool PuedeParsar(string texto);
        Factura Parsear(string texto, string rutaArchivo, bool viaOcr);

        // Modo de extracción preferido
        // Por defecto Ordenado, cada parser puede sobreescribirlo
        ModoExtraccion ModoExtraccion =>
            ModoExtraccion.Ordenado;
    }
}
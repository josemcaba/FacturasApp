using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    public interface IInvoiceParser
    {
        string Nombre { get; }
        bool PuedeParsar(string texto);
        Factura Parsear(string texto, string rutaArchivo, bool viaOcr);

        // Modo de extracción preferido
        // Por defecto LayoutAnalysis, cada parser puede sobreescribirlo
        PdfTextExtractor.ModoExtraccion ModoExtraccion =>
            PdfTextExtractor.ModoExtraccion.LayoutAnalysis;
    }
}
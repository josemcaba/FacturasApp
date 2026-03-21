using FacturasApp.Models;

namespace FacturasApp.Services.Parsers
{
    // Interfaz paralela a IInvoiceParser pero para fuentes Excel
    public interface IExcelInvoiceParser
    {
        string Nombre { get; }

        // Identifica si este parser es el adecuado para el archivo Excel
        bool PuedeParsar(string rutaExcel);

        // Extrae las facturas del Excel
        List<Factura> Parsear(string rutaExcel);
    }
}
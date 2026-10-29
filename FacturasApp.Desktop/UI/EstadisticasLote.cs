using FacturasApp.Core.Models;

namespace FacturasApp.UI;

/// <summary>
/// Cifras de un estado dentro del lote.
/// </summary>
/// <param name="Estado">Estado al que corresponden las cifras.</param>
/// <param name="Filas">Nº de filas del grid en ese estado (una por línea de factura).</param>
/// <param name="Documentos">Nº de facturas distintas: las líneas de una factura multilínea cuentan una sola vez.</param>
/// <param name="Importe">Suma de <see cref="Factura.ImporteVerificacion"/> de esas filas.</param>
/// <param name="PorcentajeFilas">Porcentaje sobre el total de filas del lote.</param>
/// <param name="PorcentajeDocumentos">Porcentaje sobre el total de documentos del lote.</param>
public sealed record EstadoEstadistica(
    EstadoFactura Estado,
    int Filas,
    int Documentos,
    decimal Importe,
    double PorcentajeFilas,
    double PorcentajeDocumentos);

/// <summary>
/// Estadística del lote COMPLETO de facturas (nunca del grid filtrado).
///
/// El <see cref="Factura.Estado"/> es único por factura —lo unifica
/// <c>UnificarEstadoMultiLinea()</c> tras validar—, así que las líneas de una
/// factura multilínea comparten estado y documento: aquí se cuentan como una
/// sola factura, con el mismo criterio que el detector de duplicados del
/// procesador (número + ruta del documento).
/// </summary>
public sealed class EstadisticasLote
{
    /// <summary>
    /// Estados que se muestran, de mejor a peor (mismo orden que la jerarquía
    /// de <c>UnificarEstadoMultiLinea</c>, pero con el mejor primero).
    /// </summary>
    public static readonly EstadoFactura[] OrdenEstados =
    {
        EstadoFactura.OK,
        EstadoFactura.Revisar,
        EstadoFactura.Duplicada,
        EstadoFactura.Error,
        EstadoFactura.Pendiente
    };

    /// <summary>Una entrada por estado, en el orden de <see cref="OrdenEstados"/>.</summary>
    public IReadOnlyList<EstadoEstadistica> Estados { get; }

    /// <summary>Filas totales del lote (todas las líneas).</summary>
    public int TotalFilas { get; }

    /// <summary>Facturas distintas del lote (documentos únicos).</summary>
    public int TotalDocumentos { get; }

    /// <summary>Suma de importes de las facturas que no están en Error.</summary>
    public decimal ImporteSinErrores { get; }

    /// <summary>Filas cuyo texto se extrajo por OCR.</summary>
    public int ExtraidasPorOcr { get; }

    private EstadisticasLote(IReadOnlyList<EstadoEstadistica> estados,
        int totalFilas, int totalDocumentos, decimal importeSinErrores,
        int extraidasPorOcr)
    {
        Estados = estados;
        TotalFilas = totalFilas;
        TotalDocumentos = totalDocumentos;
        ImporteSinErrores = importeSinErrores;
        ExtraidasPorOcr = extraidasPorOcr;
    }

    public static EstadisticasLote Calcular(IEnumerable<Factura> facturas)
    {
        var lista = facturas as List<Factura> ?? facturas.ToList();

        var filas = new Dictionary<EstadoFactura, int>();
        var importes = new Dictionary<EstadoFactura, decimal>();
        var documentos = new Dictionary<string, (EstadoFactura Estado, decimal Importe)>(
            StringComparer.Ordinal);

        for (int i = 0; i < lista.Count; i++)
        {
            Factura factura = lista[i];

            filas[factura.Estado] = filas.GetValueOrDefault(factura.Estado) + 1;
            importes[factura.Estado] =
                importes.GetValueOrDefault(factura.Estado) + factura.ImporteVerificacion;

            string clave = ClaveDocumento(factura, i);
            if (documentos.TryGetValue(clave, out var documento))
                documentos[clave] = (Peor(documento.Estado, factura.Estado),
                    documento.Importe + factura.ImporteVerificacion);
            else
                documentos[clave] = (factura.Estado, factura.ImporteVerificacion);
        }

        var documentosPorEstado = documentos.Values
            .GroupBy(d => d.Estado)
            .ToDictionary(g => g.Key, g => g.Count());

        int totalFilas = lista.Count;
        int totalDocumentos = documentos.Count;

        var estados = OrdenEstados
            .Select(estado => new EstadoEstadistica(
                estado,
                filas.GetValueOrDefault(estado),
                documentosPorEstado.GetValueOrDefault(estado),
                importes.GetValueOrDefault(estado),
                Porcentaje(filas.GetValueOrDefault(estado), totalFilas),
                Porcentaje(documentosPorEstado.GetValueOrDefault(estado), totalDocumentos)))
            .ToList();

        return new EstadisticasLote(estados, totalFilas, totalDocumentos,
            lista.Where(f => f.Estado != EstadoFactura.Error)
                 .Sum(f => f.ImporteVerificacion),
            lista.Count(f => f.ExtractedByOcr));
    }

    /// <summary>
    /// Clave del documento al que pertenece una fila, con el mismo criterio
    /// que <c>InvoiceProcessorService.MismaFactura</c>: número + ruta del
    /// documento. Sin número no hay forma de agrupar, así que cada fila es su
    /// propio documento.
    /// </summary>
    private static string ClaveDocumento(Factura factura, int indice)
    {
        string numero = (factura.NumeroFactura ?? string.Empty).Trim().ToUpperInvariant();
        if (numero.Length == 0) return $"~{indice}";

        return numero + "|" + RutaDocumento(factura);
    }

    private static string RutaDocumento(Factura factura)
    {
        string ruta = (factura.RutaArchivo ?? string.Empty).Trim();
        if (ruta.Length == 0) return string.Empty;

        try { return Path.GetFullPath(ruta).ToUpperInvariant(); }
        catch { return ruta.ToUpperInvariant(); }
    }

    /// <summary>El peor de los dos estados: Error &gt; Duplicada &gt; Revisar &gt; OK &gt; Pendiente.</summary>
    private static EstadoFactura Peor(EstadoFactura a, EstadoFactura b) =>
        Prioridad(a) >= Prioridad(b) ? a : b;

    private static int Prioridad(EstadoFactura estado) => estado switch
    {
        EstadoFactura.Error => 4,
        EstadoFactura.Duplicada => 3,
        EstadoFactura.Revisar => 2,
        EstadoFactura.OK => 1,
        _ => 0
    };

    private static double Porcentaje(int parte, int total) =>
        total == 0 ? 0 : parte * 100.0 / total;
}

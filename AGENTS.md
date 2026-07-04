# AGENTS.md

## Project Overview

FacturasApp es una aplicación Windows Forms (.NET 10) para procesar y gestionar facturas. Extrae datos de PDFs (texto seleccionable y escaneados vía OCR) y Excel, usando parsers específicos por proveedor.

## Build & Run

```bash
dotnet build
dotnet run
```

No hay tests configurados (carpeta `Tests/` vacía). No hay CI/CD ni linting configurado.

## Architecture

- **Entry point**: `Program.cs` → `MainForm`
- **Core service**: `Services/InvoiceProcessorService.cs` orquesta todo el flujo de procesamiento
- **Parsers**: `Services/Parsers/` — 17+ parsers específicos por proveedor + `GenericParser` como fallback
- **Parser selection**: `ParserFactory.cs` detecta el emisor y selecciona el parser correcto
- **OCR**: `OcrExtractor.cs` y `OcrZonalExtractor.cs` usan Tesseract con datos en `tessdata/`
- **Templates**: `Data/plantillas_ocr.xml` (recurso embebido) define zonas de extracción por proveedor
- **Models**: `Factura.cs`, `Empresa.cs` (base de Proveedor/Cliente), `EstadoFactura.cs`, `PlantillaOcr.cs`

## Key Patterns

- Los parsers heredan de `BaseParser` (implementa `IInvoiceParser`)
- `BaseParser` provee helpers: `CrearFacturaBase()`, `ExtraerNif()`, `ExtraerDecimal()`, `ExtraerFecha()`, `EliminarDuplicadosNoNumericos()`, `EliminarDuplicadosNumericos()`
- Los parsers declaran `protected override string[] Identificadores` → `PuedeParsar()` se hereda de BaseParser
- `ParsearMultiple()` permite que un PDF contenga varias facturas (ej: Mercadona)
- El flujo: detectar tipo PDF → identificar emisor → extracción zonal (si hay plantilla) → fallback a texto completo → parsear
- `EstadoFacturaExtensions.cs` centraliza colores y textos de display para el enum `EstadoFactura`
- La tolerancia para comparar totales es de 0.01€ (`Factura.TotalesCoinciden`)

## Conventions

- Código en español (nombres de variables, comentarios, archivos)
- Namespace `FacturasApp.Models`, `FacturasApp.Services`, `FacturasApp.UI`
- Nullable enable, ImplicitUsings habilitado
- Windows Forms con Designer files (`*.Designer.cs`, `*.resx`)

## Dependencies

ClosedXML (Excel), CsvHelper, PdfPig (PDF text), PDFtoImage, Tesseract (OCR)

## Gotchas

- `tessdata/` debe estar en el directorio de ejecución (archivos `eng.traineddata`, `spa.traineddata`)
- `plantillas_ocr.xml` está embebido como recurso en el ensamblado
- `Proveedor` y `Cliente` son subclases vacías de `Empresa` (solo por claridad semántica)
- El enum `EstadoFactura` está en `Models/` (no en Services/)
- Para agregar un nuevo parser: heredar de `BaseParser`, declarar `Nombre`, `Nif`, `Identificadores`, y sobreescribir `Parsear()`

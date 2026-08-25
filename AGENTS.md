# AGENTS.md

## Build & Run

```bash
"/mnt/c/Program Files/dotnet/dotnet.exe" build
"/mnt/c/Program Files/dotnet/dotnet.exe" run --project FacturasApp.Web/FacturasApp.Web.csproj
```

Target: **.NET 10** (net10.0-windows, SDK de los repos oficiales de dotnet).

No tests, CI/CD, linting, or typecheck configured.

### Proyectos de la solución

| Proyecto | Tipo | Target |
|---|---|---|
| `FacturasApp.Core/` | Biblioteca de clases (Library) | net10.0 |
| `FacturasApp.Web/` | ASP.NET Core Web API | net10.0-windows |

### Web API

- Escucha en `http://localhost:5000`
- Windows Service: `builder.Host.UseWindowsService()` — se puede registrar con `sc.exe`
- Frontend: SPA en `wwwroot/` (index.html + js/app.js + css/style.css)
- API: `POST /api/facturas/upload`, `POST /api/facturas/process`, `GET /api/facturas`, `GET /api/facturas/resumen`, `GET /api/facturas/export/ingresos|gastos`, `DELETE /api/facturas`, `DELETE /api/facturas/{index}`
- Almacenamiento: `static List<Factura>` con `lock` (sin base de datos)

## Key Architecture

- **Orchestrator**: `InvoiceProcessorService` — PDF→text extraction → emitter detection → parser dispatch
- **Parser dispatch**: `ParserFactory` first checks XML configs from `ConfiguracionEmisores`, then falls back to hardcoded C# parsers. `GenericParser.PuedeParsar() => true` (always fallback). ParserFactory has a ~50-entry hardcoded list — missing a registration means the C# parser is unreachable.
- **Config-driven parser**: `ConfigurableParserEngine` replaces C# parsers with XML. Each emitter = one `{Nif}.xml` stored at `%APPDATA%/FacturasApp/Emisores/`. C# parsers still work during migration.
- **Emisor XML deployment flow**: Source XMLs live in `Data/Emisores/` as embedded resources (`FacturasApp.Core.Data.Emisores.*`), auto-extracted to `%APPDATA%/FacturasApp/Emisores/` on first run via `ConfiguracionEmisores.ExtraerEmisoresPorDefecto()`. Users edit the AppData copies, not the embedded originals.
- **Text extraction**: `WebTextExtractor` (PDFium, 2 modes: `Simple`, `Ordenado`). `Simple` → `PdfDocument.GetPdfText(page)` (texto limpio, orden de contenido, saltos de línea). `Ordenado` (default) → `GetCharacterInformation(page)` con agrupación por anclas encadenadas (tolerancia 4pt sobre `Bounds.Y + Bounds.Height`, bottom del glifo, Y desde abajo) y orden por `Bounds.X`.
- **OCR path**: PDFs without selectable text → rendered via PDFium → Tesseract 5.2.0 with `spa` language. `tessdata/` (eng+spa) referenced from root via `../tessdata/`.
- **OCR uses only `spa`** despite both `eng.traineddata` and `spa.traineddata` being deployed.
- **PDFium native**: `PdfiumViewer.Updated 2.14.5` (wrapper) + `bblanchon.PDFium.Win32 139.0.7215` (nativo, deployado en `runtimes/win-x64/native/pdfium.dll`; el fork lo resuelve solo). No hay PdfPig ni PDFtoImage/SkiaSharp.
- **Zonal extraction**: `PlantillaOcrService` loads `Data/plantillas_ocr.xml` embedded resource, copies to `%APPDATA%/FacturasApp/plantillas_ocr.xml` on first run (hash-tracked for updates). Zonal coordinates in XML are percentages (0–100), not PDF points. Zones can also be defined in `{Nif}.xml` via `<ZonasOcr>`.
- **State determination**: `Services/FacturaEstado` — checks total match (tolerance 0.01€), base ≠ 0, valid NIFs, required fields, client name ≤ 40 chars.
- **Export**: `ExportService` writes Excel via ClosedXML 0.105.0, splitting OK vs non-OK into separate sheets (ingresos/gastos).

## Namespace Conventions

The project uses a mix of styles — match the existing convention for the directory you're editing:

| Directory | Style |
|---|---|
| `Models/*.cs` | Brace `namespace FacturasApp.Models { }` |
| `Models/EmisoresConfig/*.cs` | File-scoped `namespace FacturasApp.Models.EmisoresConfig;` |
| `Services/Parsers/*.cs` | Brace `namespace FacturasApp.Services.Parsers { }` |

## Conventions

- Code in Spanish (names, comments, files)
- Commitear es responsabilidad del usuario: no hacer commits ni estar pendiente de asuntos de commit
- `Proveedor` and `Cliente` are empty subclasses of `Empresa` (semantic clarity only)
- Nullable enable, ImplicitUsings enabled
- Target framework: `.NET 10` (`net10.0-windows`)
- `.slnx` format (new .NET XML solution format)

## Key Packages (csproj)

| Package | Version | Project |
|---|---|---|
| ClosedXML | 0.105.0 | Core |
| PdfiumViewer.Updated | 2.14.5 | Web |
| bblanchon.PDFium.Win32 | 139.0.7215 | Web |
| Tesseract | 5.2.0 | Web |
| Microsoft.Extensions.Hosting.WindowsServices | 10.0.0 | Web |

## Gotchas

- `ConfigurableParserEngine` creates regexes with `IgnoreCase` by default (matches both "Factura" and "factura")
- `Factura.TotalesCoinciden` tolerance is 0.01€
- `Factura.TotalCalculado` = Base + CuotaIVA − CuotaIRPF + CuotaRE: si el XML extrae IRPF y la factura imprime el total BRUTO (sin descontar IRPF), el estado será Error
- `ConfigurableParserEngine` regexes NO usan flag `Multiline` (`^`/`$` no funcionan por línea)
- Los XML que usan `<ModoExtraccion>Simple</ModoExtraccion>` reciben texto de `GetPdfText`: línea "Documento Fecha" + número a continuación; `\r\n` como saltos de línea reales
- En el texto de `GetPdfText` algunas palabras salen fusionadas sin espacio (ej. "Facturaen Euro") y prefijos colapsados (ej. "FACTURA500949502")
- Regex con `(?<!\d)` en un XML debe escribirse escapado: `(?&lt;!\d)`
- `GenericParser` extracts NIF from text via regex (since `Nif` property is "General")
- Adding a C# parser requires registering it in both `ParserFactory` constructor list AND the `ConfigurableParserEngine` check (if applicable)
- Zonal coordinates in `plantillas_ocr.xml` and `ZonasOcr` configs are percentages (0–100), not PDF points
- `tessdata/` ships both `eng` and `spa` but OCR only uses `spa`
- Embedded resource logical names use dots: `FacturasApp.Core.Data.Emisores.{Filename}.xml` and `FacturasApp.Core.Data.plantillas_ocr.xml`
- `ExtraerEmisoresPorDefecto` solo extrae si el archivo NO existe en AppData: los cambios en `Data/Emisores/*.xml` deben copiarse manualmente a `%APPDATA%/FacturasApp/Emisores/`
- `ExtraerFecha` (general) devuelve null si encuentra 2+ fechas distintas (ej. un teléfono "951.91.63.89" rompe la extracción) → usar regex de fecha explícita en el XML

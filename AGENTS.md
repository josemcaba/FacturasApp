# AGENTS.md

## Build & Run

```bash
"/mnt/c/Program Files/dotnet/dotnet.exe" build
"/mnt/c/Program Files/dotnet/dotnet.exe" run
```

**Gotcha**: `dotnet build` fails with `MSB3021` if `FacturasApp.exe` is still running. Kill first:
```bash
taskkill /F /IM FacturasApp.exe 2>nul
```

No tests, CI/CD, linting, or typecheck configured.

## Key Architecture

- **Entry point**: `Program.cs` → `MainForm` (WinForms)
- **Orchestrator**: `InvoiceProcessorService` — PDF→text extraction → emitter detection → parser dispatch
- **Parser dispatch**: `ParserFactory` first checks XML configs from `ConfiguracionEmisores`, then falls back to hardcoded C# parsers. `GenericParser.PuedeParsar() => true` (always fallback). ParserFactory has a ~50-entry hardcoded list — missing a registration means the C# parser is unreachable.
- **Config-driven parser**: `ConfigurableParserEngine` replaces C# parsers with XML. Each emitter = one `{Nif}.xml` stored at `%APPDATA%/FacturasApp/Emisores/`. C# parsers still work during migration.
- **Emisor XML deployment flow**: Source XMLs live in `Data/Emisores/` as embedded resources (`FacturasApp.Data.Emisores.*`), auto-extracted to `%APPDATA%/FacturasApp/Emisores/` on first run via `ConfiguracionEmisores.ExtraerEmisoresPorDefecto()`. Users edit the AppData copies, not the embedded originals.
- **Text extraction**: `PdfTextExtractor` (PDFium, 3 modes: `Simple`, `OrdenadoPosicion`, `LayoutAnalysis`). `Simple`/`LayoutAnalysis` → `PdfDocument.GetPdfText(page)` (texto limpio, orden de contenido, saltos de línea). `OrdenadoPosicion` (default) → `GetCharacterInformation(page)` con agrupación por anclas encadenadas (tolerancia 4pt sobre `Bounds.Y + Bounds.Height`, bottom del glifo, Y desde abajo) y orden por `Bounds.X`.
- **OCR path**: PDFs without selectable text → rendered via PDFium → Tesseract 5.2.0 with `spa` language. `OcrBase` provides shared engine setup. `tessdata/` (eng+spa) copied to output dir via `<Content>` in csproj.
- **OCR uses only `spa`** despite both `eng.traineddata` and `spa.traineddata` being deployed.
- **PDF rendering**: `OcrBase.RenderizarPaginas()`/`RenderizarPagina()`/`RenderizarPaginaReducida()` y el preview de `GestionEmisoresForm.CargarPdfMuestra` usan `PdfDocument.Render(page, w, h, dpiX, dpiY, PdfRenderFlags.ForPrinting)` con `w/h = PageSizes × dpi/72` (DPI 300 OCR, preview 300). El Bitmap devuelto NO debe dispose-se en el método: el llamador es responsable.
- **PDFium native**: `PdfiumViewer.Updated 2.14.5` (wrapper) + `bblanchon.PDFium.Win32 139.0.7215` (nativo, deployado en `runtimes/win-x64/native/pdfium.dll`; el fork lo resuelve solo). No hay PdfPig ni PDFtoImage/SkiaSharp.
- **Zonal extraction**: `PlantillaOcrService` loads `Data/plantillas_ocr.xml` embedded resource, copies to `%APPDATA%/FacturasApp/plantillas_ocr.xml` on first run (hash-tracked for updates). Zonal coordinates in XML are percentages (0–100), not PDF points. Zones can also be defined in `{Nif}.xml` via `<ZonasOcr>`.
- **State determination**: `Services/FacturaEstado` — checks total match (tolerance 0.01€), base ≠ 0, valid NIFs, required fields, client name ≤ 40 chars.
- **Export**: `ExportService` writes Excel via ClosedXML 0.105.0, splitting OK vs non-OK into separate sheets (ingresos/gastos).

### GestionEmisoresForm (editor de emisores)

- **Layout**: `picFactura` maintains A4 proportion: `Width = Height * 0.707071`. `panelCentral.Dock = Left`, `panelDerecho.Dock = Fill`. All controls (btnCargarPdfMuestra, tabPaginas, picFactura) aligned and sized in `PanelCentral_Resize` handler.
- **Zone drawing**: Drag rectangles on `picFactura` to define OCR zones. Coordinates are percentage-based (0–100, same as plantillas_ocr.xml). Zones sync bidirectionally with `dgvZonas` via `_zonasDibujo` list + `_sincronizando` guard flag.

## Namespace Conventions

The project uses a mix of styles — match the existing convention for the directory you're editing:

| Directory | Style |
|---|---|
| `UI/*.cs` | File-scoped `namespace FacturasApp.UI;` |
| `Services/*.cs` | Brace `namespace FacturasApp.Services { }` |
| `Models/*.cs` | Brace `namespace FacturasApp.Models { }` |
| `Models/EmisoresConfig/*.cs` | File-scoped `namespace FacturasApp.Models.EmisoresConfig;` |
| `Services/Parsers/*.cs` | Brace `namespace FacturasApp.Services.Parsers { }` |

## Conventions

- Code in Spanish (names, comments, files)
- `Proveedor` and `Cliente` are empty subclasses of `Empresa` (semantic clarity only)
- Nullable enable, ImplicitUsings enabled
- WinForms with Designer files (`*.Designer.cs`, `*.resx`)
- `.slnx` format (new .NET XML solution format)
- App icon: `Assets/app-icon.ico`
- Custom using alias in `OcrBase.cs`: `using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;`

## Key Packages (csproj)

| Package | Version |
|---|---|
| ClosedXML | 0.105.0 |
| PdfiumViewer.Updated | 2.14.5 |
| bblanchon.PDFium.Win32 | 139.0.7215 |
| Tesseract | 5.2.0 |
| CsvHelper | 33.1.0 |

## Gotchas

- `ConfigurableParserEngine` creates regexes with `IgnoreCase` by default (matches both "Factura" and "factura")
- `Factura.TotalesCoinciden` tolerance is 0.01€
- `Factura.TotalCalculado` = Base + CuotaIVA − CuotaIRPF + CuotaRE: si el XML extrae IRPF y la factura imprime el total BRUTO (sin descontar IRPF), el estado será Error
- `ConfigurableParserEngine` regexes NO usan flag `Multiline` (`^`/`$` no funcionan por línea)
- Los XML que usan `<ModoExtraccion>LayoutAnalysis</ModoExtraccion>` reciben texto de `GetPdfText`: línea "Documento Fecha" + número a continuación; `\r\n` como saltos de línea reales
- En el texto de `GetPdfText` algunas palabras salen fusionadas sin espacio (ej. "Facturaen Euro") y prefijos colapsados (ej. "FACTURA500949502")
- Regex con `(?<!\d)` en un XML debe escribirse escapado: `(?&lt;!\d)`
- `GenericParser` extracts NIF from text via regex (since `Nif` property is "General")
- Adding a C# parser requires registering it in both `ParserFactory` constructor list AND the `ConfigurableParserEngine` check (if applicable)
- Zonal coordinates in `plantillas_ocr.xml` and `ZonasOcr` configs are percentages (0–100), not PDF points
- `tessdata/` ships both `eng` and `spa` but OCR only uses `spa`
- Embedded resource logical names use dots: `FacturasApp.Data.Emisores.{Filename}.xml` and `FacturasApp.Data.plantillas_ocr.xml`
- `ExtraerEmisoresPorDefecto` solo extrae si el archivo NO existe en AppData: los cambios en `Data/Emisores/*.xml` deben copiarse manualmente a `%APPDATA%/FacturasApp/Emisores/`
- `ExtraerFecha` (general) devuelve null si encuentra 2+ fechas distintas (ej. un teléfono "951.91.63.89" rompe la extracción) → usar regex de fecha explícita en el XML

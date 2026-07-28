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
- **Text extraction**: `PdfTextExtractor` (PdfPig, 3 modes: `Simple`, `OrdenadoPosicion`, `LayoutAnalysis`). Default is `OrdenadoPosicion`.
- **OCR path**: PDFs without selectable text → rendered via PDFtoImage 5.2.0 → Tesseract 5.2.0 with `spa` language. `OcrBase` provides shared engine setup. `tessdata/` (eng+spa) copied to output dir via `<Content>` in csproj.
- **OCR uses only `spa`** despite both `eng.traineddata` and `spa.traineddata` being deployed.
- **PDF rendering**: `OcrBase.RenderizarPaginas()` uses `PDFtoImage.Conversion.ToImage()` + SkiaSharp with DPI 300. `Index` type from PDFtoImage (or `System.Index`).
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
| PdfPig | 0.1.13 |
| PDFtoImage | 5.2.0 |
| Tesseract | 5.2.0 |
| CsvHelper | 33.1.0 |

## Gotchas

- `ConfigurableParserEngine` creates regexes with `IgnoreCase` by default (matches both "Factura" and "factura")
- `Factura.TotalesCoinciden` tolerance is 0.01€
- `GenericParser` extracts NIF from text via regex (since `Nif` property is "General")
- Adding a C# parser requires registering it in both `ParserFactory` constructor list AND the `ConfigurableParserEngine` check (if applicable)
- Zonal coordinates in `plantillas_ocr.xml` and `ZonasOcr` configs are percentages (0–100), not PDF points
- `tessdata/` ships both `eng` and `spa` but OCR only uses `spa`
- Embedded resource logical names use dots: `FacturasApp.Data.Emisores.{Filename}.xml` and `FacturasApp.Data.plantillas_ocr.xml`

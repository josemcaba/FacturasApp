# AGENTS.md

## Build & Run

```bash
dotnet build
dotnet run
```

No tests, CI/CD, linting, or typecheck configured.

## Key Architecture

- **Entry point**: `Program.cs` → `MainForm` (WinForms)
- **Orchestrator**: `InvoiceProcessorService.cs` — PDF→text extraction → emitter detection → parser dispatch
- **Text extraction**: `PdfTextExtractor.cs` (PdfPig, 3 modes: `Simple`, `OrdenadoPosicion`, `LayoutAnalysis`). Each parser can override `ModoExtraccion` — default is `OrdenadoPosicion`
- **OCR path**: PDFs without selectable text → rendered via PDFtoImage → Tesseract with `spa` language. `OcrBase.cs` provides shared engine setup. `tessdata/` (eng+spa) copied to output dir via `<Content>` in csproj
- **Parser selection**: `ParserFactory.cs` holds a hardcoded list of 48 parser instances. It iterates calling `PuedeParsar()`; first match wins. `GenericParser` has `PuedeParsar() => true` (always the fallback)
- **Adding a parser**: (1) inherit `BaseParser`, set `Nombre`, `Nif`, `Identificadores`, override `Parsear()`; (2) register instance in `ParserFactory` constructor. `Nombre`/`Nif` are pre-set on `factura.Emisor` by `CrearFacturaBase()`
- **Multi-invoice PDFs**: override `ParsearMultiple()` (e.g. Mercadona returns one `Factura` per IVA line)
- **Zonal extraction**: `PlantillaOcrService` loads `plantillas_ocr.xml` embedded resource, copies to `%APPDATA%/FacturasApp/plantillas_ocr.xml` on first run (hash-tracked for updates). Zonal extraction via coordinates (selectable PDFs) or `OcrZonalExtractor` (scanned PDFs)
- **State determination**: `Services/FacturaEstado.cs` — checks total match (tolerance 0.01€), base ≠ 0, valid NIFs, required fields, client name ≤ 40 chars. `EstadoFacturaExtensions.cs` provides display text + cell colors
- **Export**: `ExportService.cs` writes Excel via ClosedXML, splitting OK vs non-OK into separate sheets (ingresos/gastos)

## Conventions

- Code in Spanish (names, comments, files)
- `Proveedor` and `Cliente` are empty subclasses of `Empresa` (semantic clarity only)
- Nullable enable, ImplicitUsings enabled
- WinForms with Designer files (`*.Designer.cs`, `*.resx`)
- `.slnx` format (new .NET XML solution format)
- App icon: `Assets/app-icon.ico`

## Gotchas

- OCR uses only `spa` language despite both `eng.traineddata` and `spa.traineddata` being deployed
- Zonal coordinates in `plantillas_ocr.xml` are percentages (0–100), not PDF points
- `ParserFactory` has a hardcoded list — missing a registration means the parser is unreachable
- `Factura.TotalesCoinciden` tolerance is 0.01€
- `GenericParser` extracts NIF from text via regex (since `Nif` property is "General")

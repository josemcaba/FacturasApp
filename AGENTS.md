# AGENTS.md

## Build & Run

```bash
"/mnt/c/Program Files/dotnet/dotnet.exe" build
"/mnt/c/Program Files/dotnet/dotnet.exe" run
```

Target: **.NET 10** (net10.0-windows, SDK de los repos oficiales de dotnet).

No tests, CI/CD, linting, or typecheck configured.

### Proyectos de la solución

| Proyecto | Tipo | Target |
|---|---|---|
| `FacturasApp.Desktop/` | WinForms (WinExe) | net10.0-windows |
| `FacturasApp.Core/` | Biblioteca de clases (Library) | net10.0 |

### Ejecutar

```bash
"/mnt/c/Program Files/dotnet/dotnet.exe" run --project FacturasApp.Desktop/FacturasApp.Desktop.csproj
```

### Rebuild falla si la app está en ejecución

Si la app está corriendo, `dotnet build --no-incremental` (o `dotnet run`) falla con warnings `MSB3061`/`MSB3026`/`MSB3027` porque `FacturasApp.exe` está bloqueado por el proceso. Cerrarla antes de rebuildar:

```bash
/mnt/c/Windows/System32/taskkill.exe /F /IM FacturasApp.exe
```

Si se ve un PID concreto en el error (ej. "blocked by FacturasApp (1808)") también puede usarse `/F /PID <pid>`. Tras el kill, rebuildar de nuevo.

## Key Architecture

- **Entry point**: `FacturasApp.Desktop/Program.cs` → `MainForm` (WinForms)
- **Orchestrator**: `InvoiceProcessorService` — PDF→text extraction → emitter detection → parser dispatch
- **Parser dispatch**: `ParserFactory` first checks XML configs from `ConfiguracionEmisores`, then falls back to hardcoded C# parsers. `GenericParser.PuedeParsar() => true` (always fallback). ParserFactory has ~35 active entries (plus ~11 commented-out) — missing a registration means the C# parser is unreachable.
- **Config-driven parser**: `ConfigurableParserEngine` replaces C# parsers with XML. Each emitter = one `{Nif}.xml` stored at `%APPDATA%/FacturasApp/Emisores/`. C# parsers still work during migration.
- **Emisor XML deployment flow**: Source XMLs live in `Data/Emisores/` as embedded resources (`FacturasApp.Core.Data.Emisores.*`), auto-extracted to `%APPDATA%/FacturasApp/Emisores/` on first run via `ConfiguracionEmisores.ExtraerEmisoresPorDefecto()`. Users edit the AppData copies, not the embedded originals.
- **Text extraction**: `PdfTextExtractor` (PDFium, 2 modes: `Simple`, `Ordenado`). `Simple` → `PdfDocument.GetPdfText(page)` (texto limpio, orden de contenido, saltos de línea). `Ordenado` (default) → `GetCharacterInformation(page)` con agrupación por anclas encadenadas (tolerancia 4pt sobre `Bounds.Y + Bounds.Height`, bottom del glifo, Y desde abajo) y orden por `Bounds.X`.
- **OCR path**: PDFs without selectable text → rendered via PDFium → Tesseract 5.2.0 with `spa` language. `OcrBase` provides shared engine setup. `tessdata/` (eng+spa) in repo root, linked to output dir via `<Content>` in csproj.
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
| `FacturasApp.Desktop/UI/*.cs` | File-scoped `namespace FacturasApp.UI;` |
| `FacturasApp.Desktop/Services/*.cs` | Brace `namespace FacturasApp.Services { }` |
| `FacturasApp.Core/Models/*.cs` | Brace `namespace FacturasApp.Core.Models { }` |
| `FacturasApp.Core/Models/EmisoresConfig/*.cs` | File-scoped `namespace FacturasApp.Core.Models.EmisoresConfig;` |
| `FacturasApp.Core/Services/*.cs` | Brace `namespace FacturasApp.Core.Services { }` |
| `FacturasApp.Core/Services/Parsers/*.cs` | Mix: most brace `namespace FacturasApp.Core.Services.Parsers { }`, `ConfigurableParserEngine` file-scoped |

## Conventions

- Code in Spanish (names, comments, files)
- Commitear es responsabilidad del usuario: no hacer commits ni estar pendiente de asuntos de commit
- `Proveedor` and `Cliente` are empty subclasses of `Empresa` (semantic clarity only)
- Nullable enable, ImplicitUsings enabled
- WinForms with Designer files (`*.Designer.cs`, `*.resx`)
- Target framework: `.NET 10` (`net10.0-windows`)
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

## Publicación (ClickOnce)

Flujo completo: `bash PublicarFacturasApp.sh` (raíz del repo, WSL/Git Bash). El script:

1. Copia `%APPDATA%/FacturasApp/Emisores/` → `Data/Emisores/` y `plantillas_ocr.xml` → `Data/` (los cambios del usuario quedan versionados en el repo).
2. Publica con el perfil `ClickOnceProfile` (Release, win-x64) → `bin/Release/net10.0-windows/win-x64/app.publish/`.
3. Copia el contenido a `Publicados en GitHub/ClickOnce/FacturasApp/` (repo `josemcaba.github.io`, rama `main`).
4. Borra versiones antiguas de `Application Files/` (conserva las 3 más recientes).
5. Commit (`--amend` solo si el último es "Actualizada FacturasApp…") + `git push --force-with-lease`.

Puntos importantes:

- **Manifiestos firmados** (`SignManifests=True`) con certificado autofirmado de firma de código en `Cert:\CurrentUser\My` (subject "CN=Jose M. Caballero", validez 3 años). Backup exportado a `Properties/FacturasAppClickOnce.pfx` (en `.gitignore`, no se comitea). Recrear si caduca o se rgenera:
  `New-SelfSignedCertificate -Type CodeSigning -Subject "CN=Jose M. Caballero" -CertStoreLocation Cert:\CurrentUser\My -KeyExportPolicy Exportable`
  y exportar el pfx. Cambiar el thumbprint en `ClickOnceProfile.pubxml`.
- **Firma ≠ SmartScreen**: el certificado autofirmado evita "Editor desconocido" en la instalación, pero Windows SmartScreen seguirá advirtiendo si exige firma de certificado comercial/trusted.
- **Bootstrapper runtime hardcodeado**: `Microsoft.NetCore.DesktopRuntime.10.0.x64` (10.0.10) en `ClickOnceProfile.pubxml` → actualizarlo cuando salgan parches nuevos del runtime .NET 10.
- **Versionado**: `<Version>3.0.0</Version>` en el csproj → `AssemblyVersion`/`FileVersion` 3.0.0.0 y ClickOnce `ApplicationVersion=3.0.0.*`. **El script incrementa `ApplicationRevision` en `ClickOnceProfile.pubxml` antes de cada publicación** (Paso 1.5), garantizando que cada push sea una versión nueva que ClickOnce detecta como actualización.
- **Auto-actualización al iniciar**: configurada en el pubxml con `UpdateEnabled=True`, `UpdateMode=Foreground`, `UpdateRequired=False`, `InstallFrom=Web`. Al arrancar, ClickOnce compara la versión del manifest del sitio con la instalada; si hay una más nueva ofrece instalarla. Si un usuario no recibe actualizaciones, revisar que la revisión de la publicación anterior sea distinta de la instalada.
- Si no hay cambios en el sitio, el script avisa y no commitea vacío.


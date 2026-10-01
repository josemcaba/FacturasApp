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
- **Zonal extraction**: Zones are defined per-emitter in `{Nif}.xml` via `<ZonasOcr>` (percentage coordinates 0–100). `InvoiceProcessorService.ObtenerPlantilla()` builds a `PlantillaOcr` from the XML config for `ConfigurableParserEngine` parsers; C# parsers have no zonal extraction.
- **State determination**: `Services/FacturaEstado` — checks total match (tolerance 0.01€), base ≠ 0, valid NIFs, required fields, client name ≤ 40 chars.
- **Export**: `ExportService` writes Excel via ClosedXML 0.105.0, splitting OK vs non-OK into separate sheets (ingresos/gastos).

### GestionEmisoresForm (editor de emisores)

- **Layout**: `picFactura` maintains A4 proportion: `Width = Height * 0.707071`. `panelCentral.Dock = Left`, `panelDerecho.Dock = Fill`. All controls (btnCargarPdfMuestra, tabPaginas, picFactura) aligned and sized in `PanelCentral_Resize` handler.
- **Zone drawing**: Drag rectangles on `picFactura` to define OCR zones. Coordinates are percentage-based (0–100, same as `ZonasOcr` configs). Zones sync bidirectionally with `dgvZonas` via `_zonasDibujo` list + `_sincronizando` guard flag.

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
- Zonal coordinates in `ZonasOcr` configs are percentages (0–100), not PDF points
- `tessdata/` ships both `eng` and `spa` but OCR only uses `spa`
- Embedded resource logical names use dots: `FacturasApp.Core.Data.Emisores.{Filename}.xml`
- `ExtraerEmisoresPorDefecto` solo extrae si el archivo NO existe en AppData: los cambios en `FacturasApp.Core/Data/Emisores/*.xml` deben copiarse manualmente a `%APPDATA%/FacturasApp/Emisores/` (o al revés con `bash ActualizarEmisores.sh`)
- `ExtraerFecha` (general) devuelve null si encuentra 2+ fechas distintas (ej. un teléfono "951.91.63.89" rompe la extracción) → usar regex de fecha explícita en el XML

## Publicación (ClickOnce)

Flujo completo: `bash PublicarFacturasApp.sh` (raíz del repo, WSL/Git Bash). El script:

0. Comprobaciones previas: rutas, `FacturasApp.exe` cerrado (si corre, aborta con el `taskkill` sugerido), MSBuild localizado (**aborta si sólo hay `dotnet.exe`: no implementa ClickOnce**), `PublishDir` leído del `.pubxml` y perfil legible.
1. Copia `%APPDATA%/FacturasApp/Emisores/*.xml` → `FacturasApp.Core/Data/Emisores/` (los cambios del usuario quedan versionados en el repo como recursos embebidos).
1.5. **Versionado**: compara la *base* de `ApplicationVersion` (3 primeros componentes) con la del manifest del sitio —la 4ª parte es la *revisión*, contador de build, y compararla entera daría falsos positivos— y avisa si es MENOR (los clientes no se actualizarían) o si coincide (sólo se verá el cambio por revisión). Avisa también si `<ApplicationVersion>` del pubxml no está alineada con `<Version>` del csproj. Luego incrementa `ApplicationRevision` si `IsRevisionIncremented=False`, o deja que MSBuild lo haga y lo verifica después.
2. Publica con el perfil `ClickOnceProfile` (Release, Any CPU) → carpeta indicada por `<PublishDir>` del pubxml (`FacturasApp.Desktop/bin/Release/net10.0-windows/app.publish/`). Usa `MSBuild.exe -t:Publish`.
3. **Copia el set ClickOnce al sitio y verifica**: `FacturasApp.application` + `setup.exe` + `Publish.html` + `Application Files/<versión construida>/`, y borra de la raíz cualquier fichero suelto no referenciado (los DLL copiados a mano se ignoran al instalar).
4. Borra versiones antiguas de `Application Files/` (conserva las 3 más recientes).
5. Commit + `git push --force-with-lease` en la rama actual del repo `josemcaba.github.io`. `--amend` sólo si el último commit empieza por "Actualizada FacturasApp…" **y todavía no está en el remoto** (así no se reescribe historia publicada).

Overrides para pruebas: `WIN_HOME=/ruta` fuerza el HOME de Windows y `MSBUILD_EXE=/ruta` fuerza el ejecutable de publicación.

Puntos importantes:

- **`PublishDir` ≠ `PublishUrl`: la copia NO la hace MSBuild**. El target `Publish` sólo rellena `PublishDir` (staging local); la copia a `PublishUrl` la realiza el diálogo *Publicar* de Visual Studio, no MSBuild. Ejecutando `MSBuild -t:Publish` desde la consola la publicación se queda en local — por eso existe el Paso 3. (Este fue el fallo del 5-sep: el manifest del sitio seguía apuntando a `_1_0_0_0` y se actualizó a mano, con el DLL suelto que ClickOnce ignora.)
- **ClickOnce publica `<ApplicationVersion>`, no `<Version>` del csproj**. El `*` se sustituye por `ApplicationRevision`, así que `1.0.0.*` ignora `<Version>3.0.0</Version>` y la app queda etiquetada `1.0.0.x` para siempre. Debe ser `<ApplicationVersion>3.0.0.*</ApplicationVersion>`, alineada con el `<Version>` del csproj. El script comprueba esto en el Paso 1.5 y lo reavisa en el Paso 3.
- **El `.pubxml` no se versiona**: `.gitignore` tiene `*.pubxml`. Si se pierde, hay que recrearlo (perfil con `PublishProtocol=ClickOnce`, `InstallFrom=Web`, `InstallUrl=https://josemcaba.github.io/ClickOnce/FacturasApp/`, `UpdateEnabled=True`, `UpdateMode=Foreground`, `ApplicationVersion=3.0.0.*`, `PublishDir` y `PublishUrl`).
- **Manifiestos sin firma**: `SignManifests=False` en el perfil actual. Si se reactiva (`True`), hace falta un certificado de firma de código en `Cert:\CurrentUser\My` (subject "CN=Jose M. Caballero") y cambiar el thumbprint en el `.pubxml`. Sin firma, Windows SmartScreen puede advertir en la instalación.
- **Bootstrapper runtime hardcodeado**: `Microsoft.NetCore.DesktopRuntime.10.0.x64` (10.0.11) en `ClickOnceProfile.pubxml` → actualizarlo cuando salgan parches nuevos del runtime .NET 10.
- **Versionado**: `<Version>3.0.0</Version>` en el csproj → `AssemblyVersion`/`FileVersion` 3.0.0.0 → publicación `3.0.0.<revisión>`. Cada publicación necesita una `ApplicationRevision` distinta: si se repite, ClickOnce no ve la actualización. El script lo garantiza (bump manual o forzado si MSBuild no lo persiste).
- **Auto-actualización al iniciar**: configurada en el pubxml con `UpdateEnabled=True`, `UpdateMode=Foreground`, `UpdateRequired=False`, `InstallFrom=Web`. Al arrancar, ClickOnce compara el manifest del sitio con la instalada y ofrece instalar la nueva. Si un usuario no recibe actualizaciones, comprobar con `curl -s https://josemcaba.github.io/ClickOnce/FacturasApp/FacturasApp.application | grep assemblyIdentity` que el `version` del sitio es MAYOR que la instalada.
- `plantillas_ocr.xml` (en `%APPDATA%/FacturasApp/`) ya no se copia al repo: no está versionado ni embebido.
- Si no hay cambios en `ClickOnce/FacturasApp/`, el script avisa y no commitea vacío (ignora cambios ajenos del repo del sitio, p. ej. `index.html`).


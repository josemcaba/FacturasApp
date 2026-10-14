# AGENTS.md

## Build & Run

```bash
"/mnt/c/Program Files/dotnet/dotnet.exe" build
"/mnt/c/Program Files/dotnet/dotnet.exe" run --project FacturasApp.Desktop/FacturasApp.Desktop.csproj
```

Target: **.NET 10** (`net10.0-windows`, SDK de los repos oficiales de dotnet). No tests, CI/CD, linting ni typecheck configurados.

| Proyecto | Tipo | Target |
|---|---|---|
| `FacturasApp.Desktop/` | WinForms (WinExe) | net10.0-windows |
| `FacturasApp.Core/` | Biblioteca de clases (Library) | net10.0 |

**El rebuild falla si la app está en ejecución** (`MSB3061`/`MSB3026`/`MSB3027`: `FacturasApp.exe` bloqueado): cerrarla antes con `/mnt/c/Windows/System32/taskkill.exe /F /IM FacturasApp.exe` y rebuildar.

## Key Architecture

- **Entry point**: `FacturasApp.Desktop/Program.cs` → `MainForm` (WinForms). **Orchestrator**: `InvoiceProcessorService` — PDF→texto → detección de emisor → dispatch de parser.
- **Parser dispatch**: `ParserFactory` comprueba primero los XML de `ConfiguracionEmisores` y luego cae a los parsers C# (`GenericParser.PuedeParsar() => true`, fallback siempre). `ConfigurableParserEngine` es el motor XML que sustituye a los parsers C#: un emisor = un `{Nif}.xml` en `%APPDATA%/FacturasApp/Emisores/` (los C# siguen funcionando durante la migración).
- **Sincronización de emisores por `<Version>` (timestamp `yyyyMMddHHmm`) — regla única**: los XML viven en `FacturasApp.Core/Data/Emisores/` como recursos embebidos (`FacturasApp.Core.Data.Emisores.{Fichero}.xml`, puntos y sin espacios) y al arranque se extraen a `%APPDATA%/FacturasApp/Emisores/` (`ConfiguracionEmisores.ExtraerEmisoresPorDefecto()` → `Extraer(directorio)`). Entonces: el sentinel `{Nif}.eliminado` gana (no se resucita) → si no existe el destino se escribe → si el embebido es **mayor** que el local se sobrescribe y lo anterior queda en `{Nif}.previa.xml.bak` → si es igual o menor, no se toca; un XML sin `<Version>` se lee como `1`. `EmisorConfig.Version` es **`long`** (un `int` desborda con 202610011551). Los usuarios editan las copias de AppData, nunca las embebidas.
- **Texto de los PDFs**: `PdfTextExtractor` (PDFium) con 2 modos: `Simple` → `GetPdfText()` (texto limpio, orden de contenido, `\r\n` reales); `Ordenado` (por defecto) → `GetCharacterInformation()` agrupando por anclas encadenadas (tolerancia 4pt sobre `Bounds.Y + Bounds.Height`, Y desde abajo) y ordenando por `Bounds.X`. Si no hay capa de texto → render con PDFium → Tesseract con **sólo `spa`** (aunque `tessdata/` en la raíz trae también `eng`; se copia al output con `<Content>` en el csproj). `OcrBase` centraliza el motor, y sus `Renderizar*()` más el preview de `GestionEmisoresForm.CargarPdfMuestra` usan `PdfDocument.Render(..., PdfRenderFlags.ForPrinting)` con `w/h = PageSizes × dpi/72` (300 DPI): **el Bitmap devuelto NO debe dispose-se en el método, lo hace el llamador**. PDFium nativo lo aporta `bblanchon.PDFium.Win32` (el fork resuelve `runtimes/win-x64/native/pdfium.dll`); no hay PdfPig ni SkiaSharp.
- **Extracción zonal**: `<ZonasOcr>` en el XML del emisor con coordenadas en **porcentaje 0-100** (no puntos PDF); `InvoiceProcessorService.ObtenerPlantilla()` arma la `PlantillaOcr` sólo para parsers XML (los C# no tienen zonas). `<ModoExtraccion>` también vale para zonas (Simple = orden nativo del PDF, Ordenado = reordenado por posición); la rama OCR no usa modos. En la GUI, `lblSeparadorRegex` indica si «Texto Extraído» es zonal y con qué modo.
- **Estado y export**: `Services/FacturaEstado` comprueba que el total cuadra (tolerancia 0,01€), base ≠ 0, NIF del emisor **verificado** (se busca por `f.Emisor.NIF` en la caché y se exige `NifEstado == "Valido"`; `Invalido`/`NoDisponible`/sin verificar → Error; si el NIF no está dado de alta no se comprueba, p. ej. el parser genérico), NIF del receptor con formato español, campos requeridos y nombre de cliente ≤ 40 caracteres (NIF español y comunitario se exigen igual). `ExportService` saca el Excel con ClosedXML en hojas separadas de ingresos/gastos según el estado sea OK o no.
- **`SubTotal` y `TotalCalculado` en facturas multilínea**. (1) El `SubTotal` de cada línea **nunca se extrae**: `ParsearMultiLinea()` ignora un `<Campo Nombre="SubTotal">` y un `<Mapeo Nombre="SubTotal">`, `AplicarPostProcesamiento()` salta las reglas con `CampoDestino="SubTotal"` si `factura.EsMultiLinea` (las de `invertirsigno` no lo llevan y siguen funcionando) y `CalcularSubTotalMultiLinea()` lo fija *después* del post-proceso como `Base + IVA − IRPF + RE` con los `Porcentaje*` de la propia línea (`Math.Round(..., 2, MidpointRounding.AwayFromZero)`, igual que `CuotaIVACalculado`). (2) **`TotalCalculado` es SIEMPRE ÚNICO por factura**: en multilínea es la suma de los subtotales calculados de las líneas (se fija con `Factura.FijarTotalCalculado()` **antes** de validar, así que todas las líneas de la misma factura comparten el mismo total) y en factura única sigue siendo `Base + CuotaIVA − CuotaIRPF + CuotaRE`; **no hay ninguna propiedad nueva para esto** (sólo un respaldo privado nullable). (3) Las líneas con `BaseImponible == 0` se **descartan**, sea cual sea el número de líneas que detecte el regex (se comprueba tras el post-procesamiento y el cálculo del subtotal); si no queda ninguna línea válida se cae a `ParsearUnica()`, igual que cuando el regex no encuentra nada. Consecuencia: `DiferenciaTotal`/`TotalesCoinciden` comparan en los dos casos **`TotalCalculado` contra `TotalFactura` extraído** (tolerancia 0,01€), el paso 1 de `FacturaEstado` mensajea en multilínea «Total factura calculado (suma de los subtotales de las líneas: X €) no coincide con el Total factura extraído (Y €)», y si el `TotalFactura` no se extrae (0) `MarcarTotalDocumentoNoExtraido()` deja las líneas en **Error** con su propio mensaje (antes Revisar). (4) El `Estado` también es **único por factura**: `UnificarEstadoMultiLinea()` (tras validar) se lleva el peor estado de todas las líneas — jerarquía del helper `Peor()`: `Error > Duplicada > Revisar > OK > Pendiente` — y copia a todas la **unión de sus `MensajeError` sin repetidos**, así el detalle de cualquier línea explica el estado de la factura; y el detector de duplicados (`InvoiceProcessorService.AddWithDuplicateDetection`, helpers `MismaFactura`/`MarcarDuplicada`) marca la **factura entera** (todas las líneas que comparten documento y número, la copia nueva y la antigua), no sólo la línea con la que se detectó. `ImporteVerificacion` sigue siendo de **línea** (`SubTotal` en multilínea, `TotalFactura` en única): lo usan el «Importe total» del resumen de `MainForm`, la columna «Total» del grid y el campo obligatorio del paso 5 (el Excel no tiene columna de total). `DetalleFacturaForm` muestra el par (`txtTotalCalculado` = `TotalCalculado`, `txtTotalExtraido` = `TotalFactura`, en rojo si `!TotalesCoinciden`) y la preview de `GestionEmisoresForm` muestra «Total Calculado» con **un solo valor** (`AñadirComun`) cuando hay líneas multilínea. En factura única el `SubTotal` extraído sigue funcionando igual.
- **Verificación NIF (local + VIES)**: botón «Verificar NIF» de `GestionEmisoresForm` → primero español con `NifValidator` (en local, sin red) y si no, IVA comunitario vía SOAP VIES de la Comisión Europea (`Services/ViesValidator`, `HttpClient` + XML de la BCL, sin paquetes nuevos; `GR`→`EL`, `GB` excluido por Brexit → `NoDisponible`). El resultado se registra en `EmisorConfig.NifEstado` ("Valido"/"Invalido"/"NoDisponible", nullable) **como cualquier otra edición: pendiente de guardar, el XML sólo se escribe al pulsar Guardar** (y el `<Version>` sólo sube entonces). Al guardar un emisor comunitario se consulta VIES si no hay resultado recién pedido (si VIES no está disponible se guarda `NoDisponible` sin bloquear). **El listado de emisores no muestra ninguna marca de estado NIF**: sólo el color del botón — verde claro = `Valido`, rojo claro = `Invalido`, amarillo claro = `NoDisponible`, color original sin verificar (hay que apagar `UseVisualStyleBackColor` para que se vea el `BackColor`).

### GestionEmisoresForm (editor de emisores)

- **Layout y zonas**: `picFactura` mantiene proporción A4 (`Width = Height * 0.707071`); `panelCentral.Dock = Left`, `panelDerecho.Dock = Fill`; el resto se alinea y dimensiona en `PanelCentral_Resize`. Las zonas se dibujan arrastrando sobre `picFactura` (coordenadas en %, igual que `ZonasOcr`) y se sincronizan en ambos sentidos con `dgvZonas` (`_zonasDibujo` + guard `_sincronizando`).
- **Caché de texto (nada de OCR al editar)**: el texto de la muestra se extrae una vez (`ExtraerTextoMuestra()`, único paso que puede pasar por Tesseract) y se cachea; alimenta la consola `txtRegexSource` (lo que ve es exactamente lo que parsea `ConfigurableParserEngine`, `[Campo]: valor` cuando es zonal) y «Valores extraídos» vía `ParsearMuestra()` (sólo regex, milisegundos), así que editar regexes nunca vuelve a pasar por OCR. `ActualizarVistaPreviaZonal()` es el único punto de refresco (consola + tabla) y `timerRefrescoValores` (500 ms) lo debouncea. **Cargar un PDF de muestra llama a `MarcarModificado()`**: `RutaPdfMuestra` queda pendiente de guardar (las cargas automáticas al abrir/descartar no marcan, porque `_cargando`).
- **Marca de muestra escaneada**: glifo `▨` ENTRE el NIF y el nombre (`A17001231 - ▨ NOMBRE`; nada parsea esa cadena, la selección va por `SelectedItem → Config.Nif`) y label `lblMuestraEscaneada` «▨ Muestra escaneada (OCR)» en la fila de estado junto a Guardar. Detección: `EsPdfEscaneado()` = `PdfTextExtractor.EsSeleccionable() == false` (umbral de 30 caracteres, **sin OCR**) con caché por ruta+tamaño+mtime y relleno en segundo plano (`Task.Run` + `BeginInvoke(FiltrarEmisores)`). `▨` (U+25A8) no está en Consolas (la fuente de la lista): GDI lo pinta por sustitución de fuente por glifo, como hacían las ✓/✗ antiguas; **los emojis no funcionan en un ListBox GDI**.
- **`dgvValoresExtraidos` arranca sin ninguna fila seleccionada** (`SinSeleccion()` = `ClearSelection()` + `CurrentCell = null`).

## Namespace Conventions

Mezcla de estilos; seguir el de la carpeta que se toque:

| Directorio | Estilo |
|---|---|
| `FacturasApp.Desktop/UI/*.cs` | File-scoped `namespace FacturasApp.UI;` |
| `FacturasApp.Desktop/Services/*.cs` | Brace `namespace FacturasApp.Services { }` |
| `FacturasApp.Core/Models/*.cs` | Brace `namespace FacturasApp.Core.Models { }` |
| `FacturasApp.Core/Models/EmisoresConfig/*.cs` | File-scoped `namespace FacturasApp.Core.Models.EmisoresConfig;` |
| `FacturasApp.Core/Services/*.cs` | Brace `namespace FacturasApp.Core.Services { }` |
| `FacturasApp.Core/Services/Parsers/*.cs` | Mix: casi todos brace, `ConfigurableParserEngine` file-scoped |

## Conventions

- Código en español (nombres, comentarios, ficheros). Nullable e ImplicitUsings activados, WinForms con ficheros de Designer (`*.Designer.cs`, `.resx`), solución en formato `.slnx`.
- **Commitear**: cuando estimes oportuno, pídeme permiso explicando brevemente el motivo; autorizado el commit, hago `push` sin preguntar de nuevo (salvo que diga «no hagas push»). Commitea siempre todo el working tree, aunque haya algo en el stage.
- **Todo commit incluye los XML de emisores**: antes de commitear, sincroniza `%APPDATA%/FacturasApp/Emisores/*.xml` → `FacturasApp.Core/Data/Emisores/` (`bash ActualizarEmisores.sh`) y comprueba en `git status` que los pendientes entren en el commit; nunca un add selectivo que los deje fuera.
- **«Publicar» ≠ «push»**: en este proyecto **publicar significa desplegar a los clientes** con `PublicarFacturasApp.sh` (ClickOnce → josemcaba.github.io). «No publiques» pide no ejecutar ese despliegue, **no** omitir el push.
- `Proveedor` y `Cliente` son subclases vacías de `Empresa` (claridad semántica; no son clases rotas).

## Key Packages (csproj)

| Package | Version |
|---|---|
| ClosedXML | 0.105.1 |
| PdfiumViewer.Updated | 2.14.5 |
| bblanchon.PDFium.Win32 | 157.0.8086 |
| Tesseract | 5.2.0 |
| CsvHelper | 33.1.0 |

`bash Scripts/ComprobarPaquetes.sh` compara cada `PackageReference` con la última **estable** de nuget.org (filtra las preview) e informa en `Scripts/informes/`. **No actualizar paquetes sin pedir permiso**: pueden requerir cambios de código.

## Gotchas

- `ConfigurableParserEngine` crea los regex con `IgnoreCase` y **sin `Multiline`** (`^`/`$` no funcionan por línea); un regex de campo necesita grupo de captura, porque el motor lee el Grupo 1. En un XML, un lookbehind hay que escribirlo escapado: `(?&lt;!\d)`.
- `Factura.TotalesCoinciden` tiene tolerancia de 0,01€. `Factura.TotalCalculado` = Base + CuotaIVA − CuotaIRPF + CuotaRE: si el XML extrae IRPF y la factura imprime el total bruto (sin descontarlo), el estado será Error.
- Los XML con `<ModoExtraccion>Simple</ModoExtraccion>` reciben texto de `GetPdfText`: con `\r\n` reales y a veces palabras fusionadas sin espacio ("Facturaen Euro") o prefijos colapsados ("FACTURA500949502").
- Añadir un parser C# exige registrarlo tanto en la lista del constructor de `ParserFactory` como en la comprobación de `ConfigurableParserEngine` (si le afecta); `GenericParser` extrae el NIF del texto con regex (por eso su propiedad `Nif` es "General").
- `ConfiguracionEmisores.Guardar()` pone el `<Version>` **automáticamente** cuando el contenido cambia de verdad (comparación ignorando `<Version>` y normalizando CRLF/LF), con `Math.Max(ahora, versionActual + 1)` para no grabar un valor menor que el de disco: editar en la GUI ya es «nueva versión» sin tocar números a mano. La copia AppData→repo (`sincronizar_emisores()` del script, también vía `bash ActualizarEmisores.sh`) es **consciente de `<Version>`**: no copia si el repo es más nuevo, y si coinciden las versiones pero difiere el contenido no copia y avisa (hay que editar desde la GUI o subir `<Version>` a mano en el repo).
- `ExtraerFecha` acepta fechas con nombre de mes según `<CulturaFecha>` del emisor (es-ES: "01 Ago 2026", "1 de agosto de 2026"; en-US: "01 Aug 2026", "August 1, 2026"), es **estricto por cultura** y tolera extensiones cortas del mes ("Sept" ≡ "Sep"). Las fechas numéricas no dependen de la cultura: se normalizan a `yyyy/MM/dd` cuando el año va de 4 dígitos al final y se leen siempre como dd/MM/yyyy. Devuelve null si encuentra 2+ fechas distintas (un teléfono "951.91.63.89" la rompe) → usar regex de fecha explícita en el XML.
- `XmlSerializer` escribe `<Prop xsi:nil="true" />` para un `DateTime?` vacío (cualquier nullable de valor), pero omite los `string` null: si un campo no debe aparecer en los XML hasta tener valor real, usar un surrogate string serializable.

## Publicación (ClickOnce)

`bash PublicarFacturasApp.sh` (raíz del repo, WSL/Git Bash). `--solo-emisores` sólo sincroniza los XML de emisores y termina sin publicar (`bash ActualizarEmisores.sh` es esa opción). El script: comprueba el entorno (`FacturasApp.exe` cerrado; MSBuild, y **aborta si sólo hay `dotnet.exe`, que no implementa ClickOnce**) → sincroniza los emisores al repo → versiona (incrementa `ApplicationRevision`) → publica con el perfil `ClickOnceProfile` vía `MSBuild -t:Publish` (staging en `<PublishDir>`) → copia el set al repo del sitio y lo verifica → borra versiones antiguas de `Application Files/` (conserva las 3 más recientes) → commit + push del repo del sitio → **tag anotado `vMAJOR.MINOR.PATCH.REVISION` del proyecto** (p. ej. `v3.2.1.17`) y su push. Los cambios sin commitear NO entran en el tag: el script los lista. Overrides para pruebas: `WIN_HOME=/ruta` y `MSBUILD_EXE=/ruta`.

Fallos que muerden de verdad en publicación:

- **`PublishDir` ≠ `PublishUrl`: la copia al sitio NO la hace MSBuild** (sólo rellena el staging local), por eso el script la hace a mano.
- **ClickOnce publica `<ApplicationVersion>`, no `<Version>` del csproj**: debe ser `3.2.1.*` alineada con el csproj, o la app se queda para siempre en `1.0.0.x` (el script lo comprueba).
- **El `.pubxml` no está versionado** (`.gitignore` tiene `*.pubxml`): si se pierde, recrearlo con `PublishProtocol=ClickOnce`, `InstallFrom=Web`, `InstallUrl=https://josemcaba.github.io/ClickOnce/FacturasApp/`, `UpdateEnabled=True`, `UpdateMode=Foreground`, `ApplicationVersion=3.2.1.*`, `IsRevisionIncremented=False`, `PublishDir` y `PublishUrl`.
- **Versionado**: `<Version>3.2.1</Version>` → `AssemblyVersion` 3.2.1.0 → publicación `3.2.1.<revisión>`. Al publicar, el target `FijarVersionDePublicacion` del csproj hornea la versión completa en `FileVersion`/`InformationalVersion`, y el título de la ventana (`MainForm.VersionPublicada()`) muestra **siempre cuatro niveles** (`V3.2.1.17` publicado, `V3.2.1.0` en un build normal). **MSBuild nunca auto-incrementa la revisión en línea de comandos (sólo el IDE)**, por eso `IsRevisionIncremented=False` y el script la sube antes de publicar; cada publicación necesita una revisión distinta o ClickOnce no ve el cambio.
- Manifiestos sin firma (`SignManifests=False`): sin certificado de firma, Windows SmartScreen puede avisar en la instalación. El bootstrapper del runtime está hardcodeado en el pubxml (`Microsoft.NetCore.DesktopRuntime.10.0.x64`) → actualizarlo con cada parche.
- Auto-actualización al iniciar: `UpdateEnabled=True`, `InstallFrom=Web`. Si alguien no recibe actualizaciones, comprobar con `curl -s https://josemcaba.github.io/ClickOnce/FacturasApp/FacturasApp.application | grep assemblyIdentity` que el `version` del sitio es MAYOR que el instalado.
- `plantillas_ocr.xml` (en `%APPDATA%/FacturasApp/`) ya no se copia al repo, no está versionado ni embebido. Y si no hay cambios en `ClickOnce/FacturasApp/`, el script avisa y no commitea vacío (ignora cambios ajenos del repo del sitio, p. ej. `index.html`).

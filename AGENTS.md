# AGENTS.md

## Architecture

.NET 10.0 solution with two projects:

- **FacturasApp.Core** — shared library: business logic, 50+ invoice parsers, models, OCR templates
- **FacturasApp.Web** — ASP.NET Core web API + static frontend (wwwroot/)

Entry point: `FacturasApp.Web/Program.cs` (listens on `http://0.0.0.0:5000`).

## Build & Run

```bash
# Local dev
dotnet build

# Docker (production)
docker compose up -d --build
```

No test project exists in this repo.

## Key Dependencies

| Project | Package | Purpose |
|---------|---------|---------|
| Core | ClosedXML | Excel import/export |
| Web | Docnet.Core | PDF text extraction |
| Web | Tesseract | OCR for scanned PDFs |
| Web | System.Drawing.Common | Image processing (requires libgdiplus on Linux) |

## Docker

Multi-stage build. Native runtime deps installed in container:
`libgdiplus`, `libtesseract-dev`, `tesseract-ocr-spa`, `libfontconfig1`.

OCR training data lives in `tessdata/` (eng + spa). Mounted into container at `/usr/share/tesseract-ocr/5/tessdata/`.

Config persisted via named volume `facturas-config` at `/root/.config/FacturasApp`.

## Invoice Processing Pipeline

`InvoiceProcessorService` orchestrates:

1. Detect if PDF has selectable text or is scanned
2. Identify emisor (issuer) via `ParserFactory`
3. Try zone extraction if template exists (coordinates or OCR zones)
4. Fallback to full text extraction if zone fails
5. Parse with emisor-specific parser → `List<Factura>`
6. Deduplicate across batch

## Parser System

- `IInvoiceParser` — interface for all parsers
- `BaseParser` — abstract base with `ParsearMultiple` support
- `ConfigurableParserEngine` — XML-driven configurable parser for new emisors
- `ParserFactory` — selects parser by matching text against known emisor patterns
- Parser configs: embedded XML in `Core/Data/Emisores/*.xml`
- OCR zone templates: `Core/Data/plantillas_ocr.xml` (embedded resource)

To add a new emisor: create XML config in `Core/Data/Emisores/` — no C# code needed if fields match standard layout.

## Conventions

- Web API controllers use static in-memory storage (no database)
- JSON serialization preserves original property casing (`PropertyNamingPolicy = null`)
- `ITextExtractor` abstraction allows different OCR strategies per platform
- `FacturasApp.Web.csproj` suppresses CA1416 (System.Drawing.Common is Windows-only per analyzer, but works on Linux via libgdiplus)

## File Structure

```
FacturasApp.Core/
  Data/           — emisor XML configs + OCR templates (embedded resources)
  Models/         — Factura, Emisor, EstadoFactura, etc.
  Services/       — business logic + parser interfaces
  Services/Parsers/ — 50+ emisor-specific parsers

FacturasApp.Web/
  Controllers/    — FacturasController (single API controller)
  Services/       — WebTextExtractor (OCR implementation)
  wwwroot/        — static frontend (HTML/CSS/JS)
```

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos de solución y csproj para restore cache
COPY FacturasApp.slnx ./
COPY FacturasApp.Core/FacturasApp.Core.csproj FacturasApp.Core/
COPY FacturasApp.Web/FacturasApp.Web.csproj FacturasApp.Web/
RUN dotnet restore

# Copiar código fuente
COPY FacturasApp.Core/ FacturasApp.Core/
COPY FacturasApp.Web/ FacturasApp.Web/
COPY tessdata/ tessdata/

# Publicar
RUN dotnet publish FacturasApp.Web/FacturasApp.Web.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Dependencias nativas: PDFium, Tesseract OCR, System.Drawing (libgdiplus)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libtesseract-dev \
    tesseract-ocr-spa \
    libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

# Copiar tessdata (traineddata de Tesseract)
COPY --from=build /src/tessdata/ /usr/share/tesseract-ocr/5/tessdata/

# Copiar aplicación publicada
COPY --from=build /app/publish/ .

EXPOSE 5000

ENTRYPOINT ["dotnet", "FacturasApp.Web.dll"]

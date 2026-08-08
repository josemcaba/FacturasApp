#!/bin/bash
#
# PublicarFacturasApp.sh
#
# Pipeline unificado de publicación ClickOnce de FacturasApp:
#   1. Sincroniza emisores y plantillas OCR desde %APPDATA% al repo del proyecto
#   2. Publica con el perfil ClickOnceProfile (Release, win-x64)
#   3. Copia app.publish al repo del sitio (josemcaba.github.io/ClickOnce/FacturasApp)
#   4. Limpia versiones antiguas (conserva las 3 más recientes)
#   5. Commit (amend si es repetición) + push con --force-with-lease
#
# Uso: bash PublicarFacturasApp.sh

set -euo pipefail

PROYECTO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Resolver el HOME de Windows (WSL: /mnt/c/Users/Jose; Git Bash: /c/Users/Jose)
if [ -d "/mnt/c/Users/Jose" ]; then
    WIN_HOME="/mnt/c/Users/Jose"
elif [ -d "$HOME" ] && [[ "$HOME" == /c/* ]]; then
    WIN_HOME="$HOME"
else
    WIN_HOME="$HOME"
fi

APPDATA_DIR="$WIN_HOME/AppData/Roaming/FacturasApp"
SITIO_DIR="$WIN_HOME/Carpeta DIGI storage/Mis Documentos (en DIGI)/PROYECTOS VISUAL STUDIO/Publicados en GitHub"
SITIO_APP="$SITIO_DIR/ClickOnce/FacturasApp"
PUBLISH_SRC="$PROYECTO_DIR/bin/Release/net10.0-windows/win-x64/app.publish"
KEEP_VERSIONS=3

log()  { echo -e "\n\033[1;36m==>\033[0m $1"; }
ok()   { echo -e "\033[1;32m  ✓\033[0m $1"; }
err()  { echo -e "\033[1;31m  ✗ $1\033[0m"; }
abort() { echo -e "\n\033[1;31m✗ ERROR:\033[0m $1"; read -rsp "ENTER para finalizar..." ; exit 1; }

[ -d "$PROYECTO_DIR/Data" ]                  || abort "No se encuentra el repo del proyecto en: $PROYECTO_DIR"
[ -d "$APPDATA_DIR" ]                        || abort "No se encuentra %APPDATA%/FacturasApp en: $APPDATA_DIR"
[ -d "$SITIO_DIR/.git" ]                     || abort "No se encuentra el repo del sitio en: $SITIO_DIR"

# ───────────────────────────────────────────────────────────────
log "Paso 1: Sincronizar Emisores y Plantillas OCR (AppData → repo)"
# ───────────────────────────────────────────────────────────────
[ -d "$APPDATA_DIR/Emisores" ] || abort "Falta $APPDATA_DIR/Emisores"
cp -r "$APPDATA_DIR/Emisores/." "$PROYECTO_DIR/Data/Emisores/"
ok "Emisores copiados"

[ -f "$APPDATA_DIR/plantillas_ocr.xml" ] || abort "Falta $APPDATA_DIR/plantillas_ocr.xml"
cp "$APPDATA_DIR/plantillas_ocr.xml" "$PROYECTO_DIR/Data/plantillas_ocr.xml"
ok "Plantillas OCR copiadas"

# ───────────────────────────────────────────────────────────────
# Paso 2: Publicar con el perfil ClickOnce
# ───────────────────────────────────────────────────────────────
MSBUILD=""
for c in \
    "/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" \
    "/mnt/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" \
    "/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" \
    "/c/Program Files/Microsoft Visual Studio/2022/Professional/MSBuild/Current/Bin/MSBuild.exe" \
    "/c/Program Files/Microsoft Visual Studio/2022/Enterprise/MSBuild/Current/Bin/MSBuild.exe" \
    "/mnt/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" \
    "/c/Program Files/dotnet/dotnet.exe" \
    "/mnt/c/Program Files/dotnet/dotnet.exe"; do
    [ -f "$c" ] && MSBUILD="$c" && break
done
[ -n "$MSBUILD" ] || abort "No se encontró MSBuild ni dotnet"

log "Paso 2: Publicando ClickOnce ($(basename "$MSBUILD"))"
cd "$PROYECTO_DIR"

# Restaurar con el RID de publicación (evita NETSDK1047)
"/mnt/c/Program Files/dotnet/dotnet.exe" restore -r win-x64

if [[ "$MSBUILD" == *dotnet.exe ]]; then
    "$MSBUILD" publish -c Release -r win-x64 -p:PublishProfile=ClickOnceProfile
else
    # Git Bash convierte /t a ruta (se usa //); MSBuild de Windows nativo requiere /t
    if [[ "$MSBUILD" == /mnt/* ]]; then
        SLASH="/"
    else
        SLASH="//"
    fi
    "$MSBUILD" "${SLASH}t:Publish" /p:Configuration=Release "/p:Platform=Any CPU" /p:PublishProfile=ClickOnceProfile "${SLASH}v:m"
fi
[ -d "$PUBLISH_SRC" ] || abort "No se generó $PUBLISH_SRC tras publicar"
ok "Publicado en $PUBLISH_SRC"

# ───────────────────────────────────────────────────────────────
# Paso 3: Copiar al repo del sitio
# ───────────────────────────────────────────────────────────────
log "Paso 3: Copiando a $SITIO_APP ..."
rm -rf "$SITIO_APP"
mkdir -p "$SITIO_APP"
cp -r "$PUBLISH_SRC/." "$SITIO_APP/"
ok "Copiado"

# ───────────────────────────────────────────────────────────────
# Paso 4: Limpiar versiones antiguas (conservar las KEEP_VERSIONS más recientes)
# ───────────────────────────────────────────────────────────────
AF_DIR="$SITIO_APP/Application Files"
if [ -d "$AF_DIR" ]; then
    mapfile -t VERSIONS < <(find "$AF_DIR" -maxdepth 1 -type d -name 'FacturasApp_*' | sort -V)
    TOTAL=${#VERSIONS[@]}
    if [ "$TOTAL" -gt "$KEEP_VERSIONS" ]; then
        for ((i=0; i<TOTAL-KEEP_VERSIONS; i++)); do
            rm -rf "${VERSIONS[$i]}"
        done
        ok "Eliminadas $((TOTAL-KEEP_VERSIONS)) versión(es) antigua(s)"
    else
        ok "No hay versiones antiguas que borrar"
    fi
fi

# ───────────────────────────────────────────────────────────────
# Paso 5: Commit + push al repositorio del sitio
# ───────────────────────────────────────────────────────────────
log "Paso 5: Commit y push a GitHub Pages..."
cd "$SITIO_DIR"
MENSAJE="Actualizada FacturasApp : $(date +'%d-%m-%Y')"

if [ -z "$(git status --porcelain)" ]; then
    ok "Sin cambios en el sitio — nada que subir"
else
    git add ClickOnce/FacturasApp/
    PREV=$(git log -1 --pretty=%s 2>/dev/null || true)
    if [[ "$PREV" == Actualizada* ]]; then
        ok "Commit actualizado (amend): $MENSAJE"
        git commit --amend -m "$MENSAJE"
    else
        ok "Nuevo commit: $MENSAJE"
        git commit -m "$MENSAJE"
    fi
    git push origin main --force-with-lease
    ok "Push completado"
fi

echo
echo -e "\033[1;32m=== Publicación completada ===\033[0m"
read -r -p "ENTER para finalizar..."
#!/bin/bash
#
# PublicarFacturasApp.sh
#
# Pipeline unificado de publicación ClickOnce de FacturasApp:
#   0. Comprobaciones previas (MSBuild disponible, app cerrada, rutas)
#   1. Sincroniza los emisores de %APPDATA% al repo del proyecto
#   1.5. Verifica el versionado (<Version> del csproj vs. versión publicada)
#        e incrementa la revisión ClickOnce si hace falta
#   2. Publica con el perfil ClickOnceProfile (Release, Any CPU)
#   3. Verifica la salida en bin/Release/net10.0-windows/app.publish
#      (PublishUrl del perfil publica directamente en el repo del sitio)
#   4. Limpia versiones antiguas de Application Files (conserva las 3 últimas)
#   5. Commit (amend si es repetición) + push con --force-with-lease
#   6. Resumen de la publicación
#
# Uso: bash PublicarFacturasApp.sh

set -euo pipefail

# ───────────────────────────────────────────────────────────────
# Rutas
# ───────────────────────────────────────────────────────────────
PROYECTO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Resolver el HOME de Windows (WSL: /mnt/c/Users/Jose; Git Bash: /c/Users/Jose)
# Se puede forzar con WIN_HOME=/ruta (útil para pruebas en sandbox)
if   [ -n "${WIN_HOME:-}" ];                 then :  # override explícito
elif [ -d "/mnt/c/Users/Jose" ];              then WIN_HOME="/mnt/c/Users/Jose"
elif [ -d "$HOME" ] && [[ "$HOME" == /c/* ]]; then WIN_HOME="$HOME"
else WIN_HOME="$HOME"
fi

APPDATA_DIR="$WIN_HOME/AppData/Roaming/FacturasApp"
SITIO_DIR="$WIN_HOME/Carpeta DIGI storage/Mis Documentos (en DIGI)/PROYECTOS VISUAL STUDIO/Publicados en GitHub"
SITIO_APP="$SITIO_DIR/ClickOnce/FacturasApp"

DESKTOP_DIR="$PROYECTO_DIR/FacturasApp.Desktop"
CSPROJ="$DESKTOP_DIR/FacturasApp.Desktop.csproj"
PUBXML="$DESKTOP_DIR/Properties/PublishProfiles/ClickOnceProfile.pubxml"
EMISORES_DEST="$PROYECTO_DIR/FacturasApp.Core/Data/Emisores"
PUBLIC_DIR="$DESKTOP_DIR/bin/Release/net10.0-windows/app.publish"

KEEP_VERSIONS=3

# ───────────────────────────────────────────────────────────────
# Utilidades
# ───────────────────────────────────────────────────────────────
log()  { echo -e "\n\033[1;36m==>\033[0m $1"; }
ok()   { echo -e "\033[1;32m  ✓\033[0m $1"; }
warn() { echo -e "\033[1;33m  !\033[0m $1"; }
err()  { echo -e "\033[1;31m  ✗ $1\033[0m"; }
abort() { echo -e "\n\033[1;31m✗ ERROR:\033[0m $1"; read -r -p "ENTER para finalizar..." || true; exit 1; }

# 3.0.0 -> 3.0.0.0 ; 3.0.0-beta -> 3.0.0.0 ; 3 -> 3.0.0.0
completar_version() {
    local -a partes
    IFS='.' read -r -a partes <<< "${1%%-*}"   # ${1%%-*} quita el sufijo -beta
    local i
    for ((i = 0; i < 4; i++)); do
        [ -z "${partes[$i]:-}" ] && partes[$i]=0
    done
    printf '%s.%s.%s.%s' "${partes[0]}" "${partes[1]}" "${partes[2]}" "${partes[3]}"
}

# devuelve "mayor" | "menor" | "igual"
comparar_version() {
    local -a va vb
    IFS='.' read -r -a va <<< "$1"
    IFS='.' read -r -a vb <<< "$2"
    local i x y
    for i in 0 1 2 3; do
        x=$((10#${va[$i]:-0})); y=$((10#${vb[$i]:-0}))
        (( x > y )) && { echo "mayor"; return; }
        (( x < y )) && { echo "menor";  return; }
    done
    echo "igual"
}

leer_xml() { grep -oP "(?<=<$2>)[^<]*(?=</$2>)" "$1" 2>/dev/null || true; }

# ───────────────────────────────────────────────────────────────
# Paso 0: Comprobaciones previas
# ───────────────────────────────────────────────────────────────
log "Paso 0: Comprobaciones previas"

[ -d "$EMISORES_DEST" ] || abort "No se encuentra la carpeta de emisores del repo en: $EMISORES_DEST"
[ -d "$APPDATA_DIR" ]   || abort "No se encuentra %APPDATA%/FacturasApp en: $APPDATA_DIR"
[ -d "$SITIO_DIR/.git" ] || abort "No se encuentra el repo del sitio en: $SITIO_DIR"
[ -f "$CSPROJ" ]       || abort "No se encuentra $CSPROJ"
[ -f "$PUBXML" ]       || abort "No se encuentra el perfil $PUBXML (está en .gitignore: hay que recrearlo si se pierde)"
ok "Rutas correctas"

# App cerrada: si corre, el rebuild falla con MSB3026/MSB3027
if [ -x "/mnt/c/Windows/System32/tasklist.exe" ]; then
    if "/mnt/c/Windows/System32/tasklist.exe" 2>/dev/null | grep -qi '^FacturasApp\.exe'; then
        warn "FacturasApp.exe está en ejecución: ciérralo (o ejecuta"
        warn "  /mnt/c/Windows/System32/taskkill.exe /F /IM FacturasApp.exe )"
        abort "Publicación cancelada para no fallar al bloquear FacturasApp.exe"
    fi
fi
ok "FacturasApp.exe no está bloqueando la compilación"

MSBUILD=""
for c in \
    "${MSBUILD_EXE:-}" \
    "/mnt/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" \
    "/mnt/c/Program Files/Microsoft Visual Studio/18/Professional/MSBuild/Current/Bin/MSBuild.exe" \
    "/mnt/c/Program Files/Microsoft Visual Studio/18/Enterprise/MSBuild/Current/Bin/MSBuild.exe" \
    "/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" \
    "/mnt/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" \
    "/mnt/c/Program Files/Microsoft Visual Studio/2022/Professional/MSBuild/Current/Bin/MSBuild.exe" \
    "/mnt/c/Program Files/Microsoft Visual Studio/2022/Enterprise/MSBuild/Current/Bin/MSBuild.exe" \
    "/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" \
    "/mnt/c/Program Files/dotnet/dotnet.exe" \
    "/c/Program Files/dotnet/dotnet.exe"; do
    [ -n "$c" ] && [ -f "$c" ] && MSBUILD="$c" && break
done
[ -n "$MSBUILD" ] || abort "No se encontró MSBuild ni dotnet"
ok "MSBuild: $MSBUILD"

case "$(basename "$MSBUILD")" in
    dotnet.exe) warn "Usando 'dotnet publish': sólo soporta ClickOnce si el SDK lo implementa. Si falla, usa el MSBuild de Visual Studio (2022 o 18)." ;;
esac

if [ "$(leer_xml "$PUBXML" SignManifests)" = "True" ]; then
    warn "SignManifests=True: hace falta el certificado de firma en Cert:\\CurrentUser\\My (sin él la publicación falla)."
fi
ok "Perfil: $(leer_xml "$PUBXML" PublishUrl)"

# ───────────────────────────────────────────────────────────────
log "Paso 1: Sincronizar Emisores (AppData → repo)"
# ───────────────────────────────────────────────────────────────
[ -d "$APPDATA_DIR/Emisores" ] || abort "Falta $APPDATA_DIR/Emisores"
N_EMISORES=$(find "$APPDATA_DIR/Emisores" -maxdepth 1 -type f -name '*.xml' | wc -l)
[ "$N_EMISORES" -gt 0 ] || abort "No hay XML de emisores en $APPDATA_DIR/Emisores"
find "$APPDATA_DIR/Emisores" -maxdepth 1 -type f -name '*.xml' -exec cp -f {} "$EMISORES_DEST/" \;
ok "$N_EMISORES emisores copiados a Core/Data/Emisores (recursos embebidos)"

# ───────────────────────────────────────────────────────────────
log "Paso 1.5: Versionado"
# ───────────────────────────────────────────────────────────────
CS_VERSION="$(completar_version "$(leer_xml "$CSPROJ" Version)")"
[ -n "$CS_VERSION" ] || abort "No se encontró <Version> en $CSPROJ"

# Versión actualmente publicada en el sitio (manifest de ClickOnce)
SITE_VERSION=""
if [ -f "$SITIO_APP/FacturasApp.application" ]; then
    SITE_VERSION="$(grep -oP '(?<=<assemblyIdentity name="FacturasApp\.exe" version=")[^"]*' \
                   "$SITIO_APP/FacturasApp.application" | head -1 || true)"
fi

echo "    <Version> del csproj .............. $CS_VERSION"
if [ -n "$SITE_VERSION" ]; then
    echo "    versión publicada en el sitio ... $SITE_VERSION"
    CMP="$(comparar_version "$CS_VERSION" "$SITE_VERSION")"
    case "$CMP" in
        mayor) ok "La versión del csproj es MAYOR: ClickOnce verá la publicación como actualización." ;;
        igual) warn "Coinciden. Cambia <Version> en el csproj si hay cambios en la app; si no, ClickOnce SÓLO verá la actualización por la revisión." ;;
        menor) warn "OJO: el csproj es MENOR que la publicada ($CS_VERSION < $SITE_VERSION): los clientes instalados no se actualizarán. Sube <Version> en $CSPROJ." ;;
    esac
else
    warn "No hay publicación previa en el sitio (primera publicación)."
fi

# Revisión ClickOnce: cada publicación debe llevar una revisión nueva,
# o los clientes no verán la actualización.
REV_ANTES="$(leer_xml "$PUBXML" ApplicationRevision)"
AUTO_REV="$(leer_xml "$PUBXML" IsRevisionIncremented)"
echo "    <ApplicationRevision> ............ ${REV_ANTES:-<sin definir>} (IsRevisionIncremented=${AUTO_REV:-True})"

if [ "${AUTO_REV:-True}" = "False" ]; then
    # Sin auto-incremento hay que subirla a mano antes de publicar
    [ -n "$REV_ANTES" ] && [[ "$REV_ANTES" =~ ^[0-9]+$ ]] || abort "No se encontró ApplicationRevision numérico en $PUBXML"
    NEW_REV=$((REV_ANTES + 1))
    sed -i "s|<ApplicationRevision>$REV_ANTES</ApplicationRevision>|<ApplicationRevision>$NEW_REV</ApplicationRevision>|" "$PUBXML"
    ok "Revisión ClickOnce (manual): $REV_ANTES → $NEW_REV"
else
    ok "La revisión la incrementa MSBuild al publicar (se verifica después)"
fi

# ───────────────────────────────────────────────────────────────
log "Paso 2: Publicando ClickOnce ($(basename "$MSBUILD"))"
# ───────────────────────────────────────────────────────────────
cd "$DESKTOP_DIR"

# Se usan -t: / -p: en lugar de /t: /p: para que WSL/Git Bash no conviertan
# los argumentos en rutas. ClickOnce NO se publica con 'dotnet publish'.
if [[ "$MSBUILD" == *dotnet.exe ]]; then
    "$MSBUILD" publish -c Release -p:PublishProfile=ClickOnceProfile
else
    "$MSBUILD" -t:Publish -p:Configuration=Release -p:Platform="Any CPU" \
               -p:PublishProfile=ClickOnceProfile -v:m
fi

# ───────────────────────────────────────────────────────────────
log "Paso 3: Verificando la publicación"
# ───────────────────────────────────────────────────────────────
# El perfil publica directamente en el repo del sitio (PublishUrl), no hay copia.
[ -d "$PUBLIC_DIR" ] || abort "No se generó $PUBLIC_DIR tras publicar"

for f in FacturasApp.application setup.exe Publish.html; do
    [ -f "$SITIO_APP/$f" ] || abort "Falta $f en el sitio tras publicar: revisa <PublishUrl> en $PUBXML"
done
ok "Manifiestos y setup presentes en $SITIO_APP"

if grep -qP '<assemblyIdentity name="FacturasApp\.exe" version="'"$CS_VERSION"'"' "$SITIO_APP/FacturasApp.application"; then
    ok "Manifest del sitio actualizado a la versión $CS_VERSION"
else
    warn "El manifest del sitio NO refleja $CS_VERSION. Revisa <Version> del csproj y <ApplicationVersion> del pubxml."
fi

# La revisión debe haber cambiado: si no, la próxima publicación repetirá versión
REV_DESPUES="$(leer_xml "$PUBXML" ApplicationRevision)"
if [ "${AUTO_REV:-True}" != "False" ] && [ "$REV_DESPUES" = "$REV_ANTES" ]; then
    NEW_REV=$(( ${REV_ANTES:-0} + 1 ))
    sed -i "s|<ApplicationRevision>$REV_ANTES</ApplicationRevision>|<ApplicationRevision>$NEW_REV</ApplicationRevision>|" "$PUBXML"
    warn "MSBuild no persistió el auto-incremento (sigue en $REV_ANTES)."
    warn "Forzada a $NEW_REV en el perfil para que la próxima publicación no repita versión."
    REV_FINAL="$NEW_REV"
elif [ "$REV_DESPUES" != "$REV_ANTES" ]; then
    ok "Revisión en el perfil: ${REV_ANTES:-?} → $REV_DESPUES (incrementada por MSBuild)"
    REV_FINAL="$REV_DESPUES"
else
    REV_FINAL="${REV_DESPUES:-?}"
fi

# ───────────────────────────────────────────────────────────────
log "Paso 4: Limpiando versiones antiguas (conservar $KEEP_VERSIONS)"
# ───────────────────────────────────────────────────────────────
AF_DIR="$SITIO_APP/Application Files"
if [ -d "$AF_DIR" ]; then
    mapfile -t VERSIONS < <(find "$AF_DIR" -maxdepth 1 -type d -name 'FacturasApp_*' | sort -V)
    TOTAL=${#VERSIONS[@]}
    if [ "$TOTAL" -gt "$KEEP_VERSIONS" ]; then
        for ((i = 0; i < TOTAL - KEEP_VERSIONS; i++)); do
            err "Eliminando $(basename "${VERSIONS[$i]}")"
            rm -rf "${VERSIONS[$i]}"
        done
        ok "Eliminadas $((TOTAL - KEEP_VERSIONS)) versión(es) antigua(s)"
    else
        ok "No hay versiones antiguas que borrar ($TOTAL en el sitio)"
    fi
else
    warn "No existe $AF_DIR"
fi

# ───────────────────────────────────────────────────────────────
log "Paso 5: Commit y push a GitHub Pages..."
# ───────────────────────────────────────────────────────────────
cd "$SITIO_DIR"
MENSAJE="Actualizada FacturasApp : $(date +'%d-%m-%Y')"
RAMA="$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo main)"
[ "$RAMA" = "HEAD" ] && RAMA="main"
ok "Rama del sitio: $RAMA"

# Sólo se mira la carpeta publicada: el repo del sitio puede tener
# otros cambios (.gitattributes, index.html...) que no son de esta app.
CAMBIOS="$(git status --porcelain -- ClickOnce/FacturasApp/)"
if [ -z "$CAMBIOS" ]; then
    ok "Sin cambios en ClickOnce/FacturasApp — nada que subir"
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
    git push origin "$RAMA" --force-with-lease
    ok "Push completado ($RAMA)"
fi

# ───────────────────────────────────────────────────────────────
echo
echo -e "\033[1;32m=== Publicación completada ===\033[0m"
echo    "  Versión ............ $CS_VERSION"
echo    "  Revisión ........... ${REV_FINAL:-?}"
echo    "  Sitio .............. $(leer_xml "$PUBXML" InstallUrl)"
echo
echo -e "  Emisores modificados en el repo del proyecto (commit aparte):"
git -C "$PROYECTO_DIR" status --porcelain -- FacturasApp.Core/Data/Emisores | sed 's/^/    /' || true
echo
read -r -p "ENTER para finalizar..." || true
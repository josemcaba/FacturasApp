#!/bin/bash
set -e

REPO_DIR="$(cd "$(dirname "$0")" && pwd)"
SRC="/mnt/c/Users/Jose/AppData/Roaming/FacturasApp/Emisores"
DST="$REPO_DIR/FacturasApp.Core/Data/Emisores"

cp "$SRC"/*.xml "$DST/"
echo "✓ Emisores actualizados ($(ls "$DST"/*.xml | wc -l) archivos)"

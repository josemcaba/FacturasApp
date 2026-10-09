#!/bin/bash
# ComprobarPaquetes.sh
# Compara las versiones de los paquetes NuGet declarados en los csproj del
# proyecto con la última versión ESTABLE publicada en nuget.org.
# El informe se muestra por pantalla y se guarda en Scripts/informes/.
# Uso: bash Scripts/ComprobarPaquetes.sh

set -u
RAIZ="$(cd "$(dirname "$0")/.." && pwd)"
INFORMES="$RAIZ/Scripts/informes"
mkdir -p "$INFORMES"
F_INFORME="$INFORMES/paquetes-ultimos.txt"
F_HISTORICO="$INFORMES/paquetes-historico.txt"

# Pares "id|version" de todos los PackageReference de los csproj
PARES=$(grep -ho '<PackageReference Include="[^"]*" Version="[^"]*"' "$RAIZ"/*/*.csproj \
    | sed -E 's/.*Include="([^"]*)" Version="([^"]*)"/\1|\2/' | sort -u)

{
    echo "=== Paquetes NuGet — $(date '+%d-%m-%Y %H:%M') ==="
    echo
    printf '%-30s %-14s %-14s %s\n' "PAQUETE" "ACTUAL" "ULTIMA" "ESTADO"
    printf '%-30s %-14s %-14s %s\n' "-------" "-------" "------" "------"
    HAY=0
    while IFS='|' read -r id ver; do
        [ -z "$id" ] && continue
        id_min=$(echo "$id" | tr '[:upper:]' '[:lower:]')
        # API flatcontainer de nuget.org: lista ordenada de versiones;
        # nos quedamos con la última estable (las preview llevan '-')
        ultima=$(curl -sf "https://api.nuget.org/v3-flatcontainer/$id_min/index.json" \
            | grep -o '"[0-9][^"]*"' | tr -d '"' | grep -v -- '-' | tail -1)
        if [ -z "$ultima" ]; then
            printf '%-30s %-14s %-14s %s\n' "$id" "$ver" "?" "AVISO: no se pudo consultar"
            continue
        fi
        if [ "$ver" = "$ultima" ]; then
            printf '%-30s %-14s %-14s %s\n' "$id" "$ver" "$ultima" "OK actualizado"
        else
            printf '%-30s %-14s %-14s %s\n' "$id" "$ver" "$ultima" "-> ACTUALIZAR"
            HAY=1
        fi
    done <<< "$PARES"
    echo
    if [ "$HAY" -eq 1 ]; then
        echo "Hay paquetes con version mas nueva estable. Pide permiso antes de"
        echo "actualizar: pueden requerir cambios de codigo (ver AGENTS.md)."
    else
        echo "Todos los paquetes estan en su ultima version estable."
    fi
} | tee "$F_INFORME"

# Copia con marca temporal para el historial (una línea resumen por ejecución)
{
    grep -E 'ACTUALIZAR|AVISO' "$F_INFORME" | sed "s/^/$(date '+%Y-%m-%d %H:%M') /"
} >> "$F_HISTORICO"

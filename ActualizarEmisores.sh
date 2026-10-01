#!/bin/bash
#
# ActualizarEmisores.sh
#
# Envoltura de conveniencia: sincroniza los XML de emisores de
#   %APPDATA%/FacturasApp/Emisores  ->  FacturasApp.Core/Data/Emisores
# SIN publicar nada. Equivale exactamente a:
#   bash PublicarFacturasApp.sh --solo-emisores
#
# La implementación vive en PublicarFacturasApp.sh (función sincronizar_emisores)
# para que ambos comandos compartan el mismo código. Es consciente de <Version>:
# nunca revierte un cambio hecho a mano en el repo.
#
# Uso: bash ActualizarEmisores.sh

set -e

exec bash "$(dirname "${BASH_SOURCE[0]}")/PublicarFacturasApp.sh" --solo-emisores "$@"

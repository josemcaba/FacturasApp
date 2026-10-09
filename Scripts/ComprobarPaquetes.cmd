@echo off
rem Lanza la comprobacion de paquetes NuGet desde el Programador de tareas.
rem El informe lo guarda el propio script en Scripts\informes\.
wsl.exe -e bash -c "cd '/mnt/c/Users/Jose/Carpeta DIGI storage/Mis Documentos (en DIGI)/PROYECTOS VISUAL STUDIO/FacturasApp' && bash Scripts/ComprobarPaquetes.sh > /dev/null"

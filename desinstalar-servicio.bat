@echo off
echo ============================================
echo  Desinstalar FacturasApp del Servicio de Windows
echo ============================================
echo.
echo  IMPORTANTE: Ejecutar este script como Administrador.
echo.

:: Verificar que se ejecuta como administrador
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Se necesitan permisos de Administrador.
    echo Haz clic derecho sobre este archivo y selecciona "Ejecutar como administrador".
    pause
    exit /b 1
)

set SERVICE_NAME=FacturasAppWeb

:: Verificar si el servicio existe
sc query %SERVICE_NAME% >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo El servicio %SERVICE_NAME% no esta instalado.
    pause
    exit /b 0
)

echo Deteniendo servicio...
sc stop %SERVICE_NAME% >nul 2>&1
timeout /t 3 /nobreak >nul

echo Eliminando servicio...
sc delete %SERVICE_NAME%

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================
    echo  Servicio eliminado correctamente.
    echo ============================================
) else (
    echo.
    echo ERROR: No se pudo eliminar el servicio.
)

pause

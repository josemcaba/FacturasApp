@echo off
echo ============================================
echo  Instalar FacturasApp como Servicio de Windows
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
set APP_PATH=%~dp0publish

:: Verificar que existe la carpeta publish
if not exist "%APP_PATH%\FacturasApp.Web.exe" (
    echo ERROR: No se encuentra FacturasApp.Web.exe en la carpeta publish.
    echo Ejecuta primero publicar.bat para generar la publicacion.
    pause
    exit /b 1
)

echo Ruta de la app: %APP_PATH%
echo Nombre del servicio: %SERVICE_NAME%
echo.

:: Eliminar servicio si ya existe
sc query %SERVICE_NAME% >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Deteniendo servicio existente...
    sc stop %SERVICE_NAME% >nul 2>&1
    timeout /t 3 /nobreak >nul
    echo Eliminando servicio existente...
    sc delete %SERVICE_NAME%
    timeout /t 2 /nobreak >nul
)

:: Registrar el servicio
echo Creando servicio...
sc create %SERVICE_NAME% binPath= "\"%APP_PATH%\FacturasApp.Web.exe\"" start= auto DisplayName= "FacturasApp Web"

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: No se pudo crear el servicio.
    pause
    exit /b 1
)

:: Configurar descripcion
sc description %SERVICE_NAME% "Servicio web de gestion de facturas - Puerto 5000"

:: Abrir puerto en el Firewall
echo.
echo Abriendo puerto 5000 en el Firewall...
netsh advfirewall firewall add rule name="FacturasApp Web (Puerto 5000)" dir=in action=allow protocol=tcp localport=5000

:: Iniciar el servicio
echo.
echo Iniciando servicio...
sc start %SERVICE_NAME%

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================
    echo  Servicio instalado y en marcha.
    echo.
    echo  Accede desde cualquier PC de la red:
    echo  http://<ip-del-servidor>:5000
    echo.
    echo  Para ver la IP del servidor, ejecuta: ipconfig
    echo ============================================
) else (
    echo.
    echo ERROR: El servicio no pudo iniciarse.
    echo Revisa el Visor de Eventos para mas detalles.
)

pause

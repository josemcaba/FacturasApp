@echo off
echo ============================================
echo  Publicacion de FacturasApp.Web
echo ============================================
echo.

set DOTNET="C:\Program Files\dotnet\dotnet.exe"
set PROJECT=FacturasApp.Web\FacturasApp.Web.csproj
set OUTPUT=publish

echo Limpiando publicacion anterior...
if exist %OUTPUT% rmdir /s /q %OUTPUT%

echo Publicando FacturasApp.Web (framework-dependent, win-x64)...
%DOTNET% publish %PROJECT% -c Release -r win-x64 --self-contained false -o %OUTPUT%

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: La publicacion ha fallado.
    pause
    exit /b 1
)

echo.
echo ============================================
echo  Publicacion completada en: %OUTPUT%\
echo.
echo  Copia esta carpeta al PC servidor.
echo ============================================
pause

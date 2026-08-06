@echo off
color 0c
echo ====================================================
echo        LIBERADOR DE PUERTOS DE UNITY (UDP 7777)
echo ====================================================
echo Buscando el proceso fantasma que secuestro el puerto...
set PID=
for /f "tokens=4" %%a in ('netstat -aon ^| findstr ":7777" ^| findstr "UDP"') do set PID=%%a

if "%PID%"=="" (
    color 0a
    echo.
    echo [EXITO] Ningun proceso esta bloqueando el puerto 7777. Todo esta libre.
) else (
    echo.
    echo [ALERTA] Se encontro que el PID: %PID% tiene secuestrado el puerto 7777.
    echo.
    echo ADVERTENCIA EXTREMA: Si tu Unity esta actualmente abierto, 
    echo matar este proceso cerrara tu Unity DE GOLPE y perderas todo 
    echo lo que no hayas guardado. Solo usa esto si Unity se congelo 
    echo o si te marca error el "Host" despues de reiniciar Play.
    echo.
    pause
    echo.
    taskkill /F /PID %PID%
    color 0a
    echo.
    echo [EXITO] El proceso %PID% fue asesinado y el puerto 7777 ha sido liberado.
)
echo.
pause

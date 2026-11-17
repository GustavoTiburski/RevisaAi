@echo off
rem Compila a versao Windows do RevisaAi e grava o resultado em build-log.txt
cd /d "%~dp0"
echo INICIO > build-log.txt
dotnet build RevisaAi\RevisaAi.csproj -f net10.0-windows10.0.19041.0 -nologo -v:m -clp:NoSummary >> build-log.txt 2>&1
echo CODIGO=%ERRORLEVEL% >> build-log.txt
echo FIM >> build-log.txt

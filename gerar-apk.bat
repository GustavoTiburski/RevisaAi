@echo off
rem Gera o arquivo .apk do RevisaAi para instalar no celular Android
dotnet publish RevisaAi\RevisaAi.csproj -f net10.0-android -c Release
echo.
echo O APK fica em: RevisaAi\bin\Release\net10.0-android\publish
echo Procure o arquivo que termina com -Signed.apk
pause

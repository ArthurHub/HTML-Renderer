@echo off

CD %~dp0

echo.
echo - Get version...
for /f %%i in ('getVer.exe ..\Source\SharedAssemblyInfo.cs') do set version=%%i
echo Version: %version%

echo.
echo.
echo - Git clone...
echo.
"C:\Program Files (x86)\Git\bin\git.exe" clone -q --branch=v1.5 https://github.com/ArthurHub/HTML-Renderer.git Release\git
xcopy Release\git\Source Release\Source /I /E
rmdir Release\git /s /q

echo.
echo.
echo - Create archive...
echo.
cd Release
..\7za.exe a "HtmlRenderer %version%.zip" **
cd..

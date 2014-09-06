@echo off

CD %~dp0


echo.
echo.
echo - DELETE RELEASE FOLDER..
rmdir Release /s /q

echo.
echo.
echo - RUN LIBS BUILD...
echo. 
CALL "1 build libs.bat"

echo.
echo.
echo - RUN DEMO BUILD...
echo.
CALL "2 build demo.bat"

echo.
echo.
set /p ask=- Builds complete, continue? (y/n)
if %ask%==n goto end

echo.
echo.
echo - RUN ARCHIVE...
echo.
CALL "3 archive.bat"

echo.
echo.
echo - RUN NUGET PACK...
echo.
CALL "4 pack nuget.bat"


echo.
echo.
echo - REMOVE FILES...
rmdir Release\Source /s /q
rmdir Release\Core /s /q
rmdir Release\WinForms /s /q
rmdir Release\WPF /s /q
rmdir Release\Mono /s /q
rmdir Release\PdfSharp /s /q
del "Release\*.exe"


:end
echo.
echo.
echo - FINISHED
pause
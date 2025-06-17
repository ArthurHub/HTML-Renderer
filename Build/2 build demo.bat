@echo off

CD %~dp0

set verb=/verbosity:minimal
set msbuild="C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\bin\MSBuild.exe"

set t_20=Configuration=Release;TargetFrameworkVersion=v2.0
set t_48=Configuration=Release;TargetFrameworkVersion=v4.8
set t_mono_20=%t_48%;DefineConstants=MONO

echo.
echo.
echo - BUILD WinForms...
echo.
%msbuild% ..\Source\Demo\WinForms\HtmlRenderer.Demo.WinForms.csproj /t:rebuild /p:%t_48%;OutputPath=..\..\..\Build\Release\Demo\WinForms %verb%

echo.
echo.
echo - BUILD Mono...
echo.

%msbuild% ..\Source\Demo\WinForms\HtmlRenderer.Demo.WinForms.csproj /t:rebuild /p:%t_mono_48%;OutputPath=..\..\..\Build\Release\Demo\Mono %verb%

echo.
echo.
echo - BUILD WPF...
echo.
%msbuild% ..\Source\Demo\WPF\HtmlRenderer.Demo.WPF.csproj /t:rebuild /p:%t_48%;OutputPath=..\..\..\Build\Release\Demo\WPF %verb%

echo.
echo - Handle outputs...
copy Release\Demo\WinForms\HtmlRendererWinFormsDemo.exe "Release\HtmlRenderer WinForms Demo.exe"
copy Release\Demo\Mono\HtmlRendererWinFormsDemo.exe "Release\HtmlRenderer Mono Demo.exe"
copy Release\Demo\WPF\HtmlRendererWpfDemo.exe "Release\HtmlRenderer WPF Demo.exe"
rmdir Release\Demo /s /q

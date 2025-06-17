@echo off

CD %~dp0

echo Set params...
set verb=/verbosity:minimal

set msbuild="C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\bin\MSBuild.exe"

set c_proj=..\Source\HtmlRenderer\HtmlRenderer.csproj
set wf_proj=..\Source\HtmlRenderer.WinForms\HtmlRenderer.WinForms.csproj
set wpf_proj=..\Source\HtmlRenderer.WPF\HtmlRenderer.WPF.csproj
set pdfs_proj=..\Source\HtmlRenderer.PdfSharp\HtmlRenderer.PdfSharp.csproj

set c_rel=Release\Core
set wf_rel=Release\WinForms
set wpf_rel=Release\WPF
set mono_rel=Release\Mono
set pdfs_rel=Release\PdfSharp

set c_out=..\..\Build\%c_rel%
set wf_out=..\..\Build\%wf_rel%
set wpf_out=..\..\Build\%wpf_rel%
set mono_out=..\..\Build\%mono_rel%
set pdfs_out=..\..\Build\%pdfs_rel%

set t_48=Configuration=Release;TargetFrameworkVersion=v4.8

set t_mono_48=%t_48%;DefineConstants=MONO


echo.
echo.
echo - BUILD Core...
echo.
%msbuild% %c_proj% /t:rebuild /p:%t_48%;OutputPath=%c_out%\NET48 %verb%

echo.
echo.
echo - BUILD WinForms...
echo.
%msbuild% %wf_proj% /t:rebuild /p:%t_48%;OutputPath=%wf_out%_t\NET48 %verb%
xcopy %wf_rel%_t\NET48\HtmlRenderer.WinForms.* %wf_rel%\NET48 /I
rmdir %wf_rel%_t /s /q

echo.
echo.
echo - BUILD WPF...
echo.
%msbuild% %wpf_proj% /t:rebuild /p:%t_48%;OutputPath=%wpf_out%_t\NET48 %verb%
xcopy %wpf_rel%_t\NET48\HtmlRenderer.WPF.* %wpf_rel%\NET48 /I
rmdir %wpf_rel%_t /s /q

echo.
echo.
echo - BUILD Mono...
echo.

%msbuild% %wf_proj% /t:rebuild /p:%t_mono_48%;OutputPath=%mono_out%_t\NET48 %verb%

xcopy %mono_rel%_t\NET48\HtmlRenderer.WinForms.* %mono_rel%\NET48 /I
rmdir %mono_rel%_t /s /q

echo.
echo.
echo - BUILD PdfSharp...
echo.
%msbuild% %pdfs_proj% /t:rebuild /p:%t_48%;OutputPath=%pdfs_out%_t\NET48 %verb%
xcopy %pdfs_rel%_t\NET48\HtmlRenderer.PdfSharp.* %pdfs_rel%\NET48 /I
rmdir %pdfs_rel%_t /s /q
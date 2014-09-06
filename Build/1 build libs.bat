@echo off

CD %~dp0

echo Set params...
set verb=/verbosity:minimal

set msbuild=C:\Windows\Microsoft.Net\Framework\v4.0.30319\MSBuild.exe

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

set t_20=Configuration=Release;TargetFrameworkVersion=v2.0
set t_30=Configuration=Release;TargetFrameworkVersion=v3.0
set t_35=Configuration=Release;TargetFrameworkVersion=v3.5;TargetFrameworkProfile=client
set t_40=Configuration=Release;TargetFrameworkVersion=v4.0;TargetFrameworkProfile=client
set t_45=Configuration=Release;TargetFrameworkVersion=v4.5

set t_mono_20=%t_20%;DefineConstants=MONO
set t_mono_35=%t_35%;DefineConstants=MONO
set t_mono_40=%t_40%;DefineConstants=MONO
set t_mono_45=%t_45%;DefineConstants=MONO


echo.
echo.
echo - BUILD Core...
echo.
%msbuild% %c_proj% /t:rebuild /p:%t_20%;OutputPath=%c_out%\NET20 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_30%;OutputPath=%c_out%\NET30 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_35%;OutputPath=%c_out%\NET35 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_40%;OutputPath=%c_out%\NET40 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_45%;OutputPath=%c_out%\NET45 %verb%

echo.
echo.
echo - BUILD WinForms...
echo.
%msbuild% %wf_proj% /t:rebuild /p:%t_20%;OutputPath=%wf_out%_t\NET20 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_35%;OutputPath=%wf_out%_t\NET35 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_40%;OutputPath=%wf_out%_t\NET40 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_45%;OutputPath=%wf_out%_t\NET45 %verb%
xcopy %wf_rel%_t\NET20\HtmlRenderer.WinForms.* %wf_rel%\NET20 /I
xcopy %wf_rel%_t\NET35\HtmlRenderer.WinForms.* %wf_rel%\NET35 /I
xcopy %wf_rel%_t\NET40\HtmlRenderer.WinForms.* %wf_rel%\NET40 /I
xcopy %wf_rel%_t\NET45\HtmlRenderer.WinForms.* %wf_rel%\NET45 /I
rmdir %wf_rel%_t /s /q

echo.
echo.
echo - BUILD WPF...
echo.
%msbuild% %wpf_proj% /t:rebuild /p:%t_30%;OutputPath=%wpf_out%_t\NET30 %verb%
%msbuild% %wpf_proj% /t:rebuild /p:%t_35%;OutputPath=%wpf_out%_t\NET35 %verb%
%msbuild% %wpf_proj% /t:rebuild /p:%t_40%;OutputPath=%wpf_out%_t\NET40 %verb%
%msbuild% %wpf_proj% /t:rebuild /p:%t_45%;OutputPath=%wpf_out%_t\NET45 %verb%
xcopy %wpf_rel%_t\NET30\HtmlRenderer.WPF.* %wpf_rel%\NET30 /I
xcopy %wpf_rel%_t\NET35\HtmlRenderer.WPF.* %wpf_rel%\NET35 /I
xcopy %wpf_rel%_t\NET40\HtmlRenderer.WPF.* %wpf_rel%\NET40 /I
xcopy %wpf_rel%_t\NET45\HtmlRenderer.WPF.* %wpf_rel%\NET45 /I
rmdir %wpf_rel%_t /s /q

echo.
echo.
echo - BUILD Mono...
echo.
%msbuild% %wf_proj% /t:rebuild /p:%t_mono_20%;OutputPath=%mono_out%_t\NET20 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_mono_35%;OutputPath=%mono_out%_t\NET35 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_mono_40%;OutputPath=%mono_out%_t\NET40 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_mono_45%;OutputPath=%mono_out%_t\NET45 %verb%
xcopy %mono_rel%_t\NET20\HtmlRenderer.WinForms.* %mono_rel%\NET20 /I
xcopy %mono_rel%_t\NET35\HtmlRenderer.WinForms.* %mono_rel%\NET35 /I
xcopy %mono_rel%_t\NET40\HtmlRenderer.WinForms.* %mono_rel%\NET40 /I
xcopy %mono_rel%_t\NET45\HtmlRenderer.WinForms.* %mono_rel%\NET45 /I
rmdir %mono_rel%_t /s /q

echo.
echo.
echo - BUILD PdfSharp...
echo.
%msbuild% %pdfs_proj% /t:rebuild /p:%t_20%;OutputPath=%pdfs_out%_t\NET20 %verb%
%msbuild% %pdfs_proj% /t:rebuild /p:%t_35%;OutputPath=%pdfs_out%_t\NET35 %verb%
%msbuild% %pdfs_proj% /t:rebuild /p:%t_40%;OutputPath=%pdfs_out%_t\NET40 %verb%
%msbuild% %pdfs_proj% /t:rebuild /p:%t_45%;OutputPath=%pdfs_out%_t\NET45 %verb%
xcopy %pdfs_rel%_t\NET20\HtmlRenderer.PdfSharp.* %pdfs_rel%\NET20 /I
xcopy %pdfs_rel%_t\NET35\HtmlRenderer.PdfSharp.* %pdfs_rel%\NET35 /I
xcopy %pdfs_rel%_t\NET40\HtmlRenderer.PdfSharp.* %pdfs_rel%\NET40 /I
xcopy %pdfs_rel%_t\NET45\HtmlRenderer.PdfSharp.* %pdfs_rel%\NET45 /I
rmdir %pdfs_rel%_t /s /q
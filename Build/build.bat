@echo off

CD %~dp0

echo Run MSBuild...

echo Delete previous folder..
rmdir Release /s /q

echo Get version...
for /f %%i in ('getVer.exe ..\Source\SharedAssemblyInfo.cs') do set version=%%i
echo Version: %version%

echo Set params...
set verb=/verbosity:minimal

set msbuild=C:\Windows\Microsoft.Net\Framework\v4.0.30319\MSBuild.exe
set git="C:\Program Files (x86)\Git\bin\git.exe"

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


echo -
echo --
echo --- Run Core builds...
%msbuild% %c_proj% /t:rebuild /p:%t_20%;OutputPath=%c_out%\NET20 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_30%;OutputPath=%c_out%\NET30 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_35%;OutputPath=%c_out%\NET35 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_40%;OutputPath=%c_out%\NET40 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_45%;OutputPath=%c_out%\NET45 %verb%

echo Run WinForms builds...
%msbuild% %wf_proj% /t:rebuild /p:%t_20%;OutputPath=%wf_out%_t\NET20 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_35%;OutputPath=%wf_out%_t\NET35 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_40%;OutputPath=%wf_out%_t\NET40 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_45%;OutputPath=%wf_out%_t\NET45 %verb%
xcopy %wf_rel%_t\NET20\HtmlRenderer.WinForms.* %wf_rel%\NET20 /I
xcopy %wf_rel%_t\NET35\HtmlRenderer.WinForms.* %wf_rel%\NET35 /I
xcopy %wf_rel%_t\NET40\HtmlRenderer.WinForms.* %wf_rel%\NET40 /I
xcopy %wf_rel%_t\NET45\HtmlRenderer.WinForms.* %wf_rel%\NET45 /I
rmdir %wf_rel%_t /s /q

echo Run WPF builds...
%msbuild% %wpf_proj% /t:rebuild /p:%t_30%;OutputPath=%wpf_out%_t\NET30 %verb%
%msbuild% %wpf_proj% /t:rebuild /p:%t_35%;OutputPath=%wpf_out%_t\NET35 %verb%
%msbuild% %wpf_proj% /t:rebuild /p:%t_40%;OutputPath=%wpf_out%_t\NET40 %verb%
%msbuild% %wpf_proj% /t:rebuild /p:%t_45%;OutputPath=%wpf_out%_t\NET45 %verb%
xcopy %wpf_rel%_t\NET30\HtmlRenderer.WPF.* %wpf_rel%\NET30 /I
xcopy %wpf_rel%_t\NET35\HtmlRenderer.WPF.* %wpf_rel%\NET35 /I
xcopy %wpf_rel%_t\NET40\HtmlRenderer.WPF.* %wpf_rel%\NET40 /I
xcopy %wpf_rel%_t\NET45\HtmlRenderer.WPF.* %wpf_rel%\NET45 /I
rmdir %wpf_rel%_t /s /q

echo Run MONO builds...
%msbuild% %wf_proj% /t:rebuild /p:%t_mono_20%;OutputPath=%mono_out%_t\NET20 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_mono_35%;OutputPath=%mono_out%_t\NET35 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_mono_40%;OutputPath=%mono_out%_t\NET40 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_mono_45%;OutputPath=%mono_out%_t\NET45 %verb%
xcopy %mono_rel%_t\NET20\HtmlRenderer.WinForms.* %mono_rel%\NET20 /I
xcopy %mono_rel%_t\NET35\HtmlRenderer.WinForms.* %mono_rel%\NET35 /I
xcopy %mono_rel%_t\NET40\HtmlRenderer.WinForms.* %mono_rel%\NET40 /I
xcopy %mono_rel%_t\NET45\HtmlRenderer.WinForms.* %mono_rel%\NET45 /I
rmdir %mono_rel%_t /s /q

echo Run PDF Sharp builds...
%msbuild% %pdfs_proj% /t:rebuild /p:%t_20%;OutputPath=%pdfs_out%_t\NET20 %verb%
%msbuild% %pdfs_proj% /t:rebuild /p:%t_35%;OutputPath=%pdfs_out%_t\NET35 %verb%
%msbuild% %pdfs_proj% /t:rebuild /p:%t_40%;OutputPath=%pdfs_out%_t\NET40 %verb%
%msbuild% %pdfs_proj% /t:rebuild /p:%t_45%;OutputPath=%pdfs_out%_t\NET45 %verb%
xcopy %pdfs_rel%_t\NET20\HtmlRenderer.PdfSharp.* %pdfs_rel%\NET20 /I
xcopy %pdfs_rel%_t\NET35\HtmlRenderer.PdfSharp.* %pdfs_rel%\NET35 /I
xcopy %pdfs_rel%_t\NET40\HtmlRenderer.PdfSharp.* %pdfs_rel%\NET40 /I
xcopy %pdfs_rel%_t\NET45\HtmlRenderer.PdfSharp.* %pdfs_rel%\NET45 /I
rmdir %pdfs_rel%_t /s /q

echo Run Demo builds...
%msbuild% ..\Source\Demo\WinForms\HtmlRenderer.Demo.WinForms.csproj /t:rebuild /p:%t_20%;OutputPath=..\..\..\Build\Release\Demo\WinForms %verb%
%msbuild% ..\Source\Demo\WinForms\HtmlRenderer.Demo.WinForms.csproj /t:rebuild /p:%t_mono_20%;OutputPath=..\..\..\Build\Release\Demo\Mono %verb%
%msbuild% ..\Source\Demo\WPF\HtmlRenderer.Demo.WPF.csproj /t:rebuild /p:%t_40%;OutputPath=..\..\..\Build\Release\Demo\WPF %verb%

echo Handle Demo output...
copy Release\Demo\WinForms\HtmlRendererWinFormsDemo.exe "Release\HtmlRenderer WinForms Demo.exe"
copy Release\Demo\Mono\HtmlRendererWinFormsDemo.exe "Release\HtmlRenderer Mono Demo.exe"
copy Release\Demo\WPF\HtmlRendererWpfDemo.exe "Release\HtmlRenderer WPF Demo.exe"
rmdir Release\Demo /s /q


echo -
echo --
set /p ask=--- Builds complete, continue? (y/n)
if %ask%==n goto end


echo Git clone...
%git% clone -q --branch=v1.5 https://github.com/ArthurHub/HTML-Renderer.git Release\git
xcopy Release\git\Source Release\Source /I /E
rmdir Release\git /s /q

echo Create archive...
cd Release
..\7za.exe a "HtmlRenderer %version%.zip" **
cd..

echo Create Core NuGets...
nuget.exe pack NuGet\HtmlRenderer.Core.nuspec -Version %version% -OutputDirectory Release

echo Create WinForms NuGets...
nuget.exe pack NuGet\HtmlRenderer.WinForms.nuspec -Version %version% -OutputDirectory Release

echo Create WPF NuGets...
nuget.exe pack NuGet\HtmlRenderer.WPF.nuspec -Version %version% -OutputDirectory Release

echo Create Mono NuGets...
nuget.exe pack NuGet\HtmlRenderer.Mono.nuspec -Version %version% -OutputDirectory Release

echo Create PdfSharp NuGets...
nuget.exe pack NuGet\HtmlRenderer.PdfSharp.nuspec -Version %version% -OutputDirectory Release



echo -
echo --
echo --- Remove files...
rmdir Release\Source /s /q
rmdir Release\Core /s /q
rmdir Release\WinForms /s /q
rmdir Release\WPF /s /q
rmdir Release\Mono /s /q
rmdir Release\PdfSharp /s /q
del "Release\HtmlRenderer WinForms Demo.exe"
del "Release\HtmlRenderer WPF Demo.exe"



:end
echo -
echo --
echo --- FINISHED
pause
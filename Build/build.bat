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

set c_out=..\..\Build\Release\Core
set wf_out=..\..\Build\Release\WinForms
set wpf_out=..\..\Build\Release\WPF

set t_20=Configuration=Release;TargetFrameworkVersion=v2.0
set t_30=Configuration=Release;TargetFrameworkVersion=v3.0
set t_35=Configuration=Release;TargetFrameworkVersion=v3.5;TargetFrameworkProfile=client
set t_40=Configuration=Release;TargetFrameworkVersion=v4.0;TargetFrameworkProfile=client
set t_45=Configuration=Release;TargetFrameworkVersion=v4.5

echo Run Core builds...
%msbuild% %c_proj% /t:rebuild /p:%t_20%;OutputPath=%c_out%\NET20 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_30%;OutputPath=%c_out%\NET30 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_35%;OutputPath=%c_out%\NET35 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_40%;OutputPath=%c_out%\NET40 %verb%
%msbuild% %c_proj% /t:rebuild /p:%t_45%;OutputPath=%c_out%\NET45 %verb%

echo Run WinForms builds...
%msbuild% %wf_proj% /t:rebuild /p:%t_20%;OutputPath=%wf_out%\NET20 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_35%;OutputPath=%wf_out%\NET35 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_40%;OutputPath=%wf_out%\NET40 %verb%
%msbuild% %wf_proj% /t:rebuild /p:%t_45%;OutputPath=%wf_out%\NET45 %verb%

echo Run WPF builds...
%msbuild% %wpf_proj% /t:rebuild /p:%t_30%;OutputPath=%wpf_out%\NET30 %verb%
%msbuild% %wpf_proj% /t:rebuild /p:%t_35%;OutputPath=%wpf_out%\NET35 %verb%
%msbuild% %wpf_proj% /t:rebuild /p:%t_40%;OutputPath=%wpf_out%\NET40 %verb%
%msbuild% %wpf_proj% /t:rebuild /p:%t_45%;OutputPath=%wpf_out%\NET45 %verb%

echo Run Demo builds...
%msbuild% ..\Source\Demo\WinForms\HtmlRenderer.Demo.WinForms.csproj /t:rebuild /p:%t_20%;OutputPath=..\..\..\Build\Release\Demo\WinForms %verb%
%msbuild% ..\Source\Demo\WPF\HtmlRenderer.Demo.WPF.csproj /t:rebuild /p:%t_40%;OutputPath=..\..\..\Build\Release\Demo\WPF %verb%

echo Handle Demo output...
copy Release\Demo\WinForms\HtmlRendererWinFormsDemo.exe "Release\HtmlRenderer WinForms Demo.exe"
copy Release\Demo\WPF\HtmlRendererWpfDemo.exe "Release\HtmlRenderer WPF Demo.exe"
rmdir Release\Demo /s /q

echo -- ? -- ? -- ? --
echo Builds complete, continue?
pause

echo Git clone...
%git% clone -q --branch=v1.5 https://github.com/ArthurHub/HTML-Renderer.git Release\git
xcopy Release\git\Source Release\Source /I /E
rmdir Release\git /s /q

echo Create archive...
cd Release
..\7za.exe a "HtmlRenderer %version%.zip" **
cd..

echo Create NuGets...
nuget.exe pack HtmlRenderer.WinForms.nuspec -Version %version% -OutputDirectory Release

echo Remove files...
rmdir Release\Source /s /q
rmdir Release\Core /s /q
rmdir Release\WinForms /s /q
rmdir Release\WPF /s /q
del "Release\HtmlRenderer WinForms Demo.exe"
del "Release\HtmlRenderer WPF Demo.exe"

echo
echo FINISHED
pause
@echo off

CD %~dp0

echo Run MSBuild...
C:\Windows\Microsoft.Net\Framework\v4.0.30319\MSBuild.exe build.xml /v:m

pause
@echo off
set startPath=%cd%
set startDrive= %startPath:~0,2% 
set fileDrive=%~dp0
%fileDrive:~0,2% 
cd %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild build.msbuild  /m:4 %*
%startDrive%
cd %startPath%
@echo off
set pkgStartPath=%cd%
set pkgStartDrive= %pkgStartPath:~0,2% 
set sourceRoot=%~dp0
set fileDrive=%~dp0
%fileDrive:~0,2% 
cd %~dp0

for /F "tokens=1* delims= " %%A in ('date /T') do set CDATE=%%B
for /F "tokens=1,2 eol=/ delims=/ " %%A in ('date /T') do set mm=%%B
for /F "tokens=1,2 delims=/ eol=/" %%A in ('echo %CDATE%') do set dd=%%B
for /F "tokens=2,3 delims=/ " %%A in ('echo %CDATE%') do set yyyy=%%B
set date=%yyyy%%mm%%dd%

rd artifacts /S /Q
set DEPLOY-FOLDER=%~dp0\artifacts\deploy
set TEMP-PACKAGE-FOLDER=Bonobo-Git\Bonobo.Git.Server\_PublishedWebsites\Bonobo.Git.Server_Package
set SERVER1-TEMP=%1
REM For multi-node deployments
REM set SERVER2-TEMP=%2
set GIT-SHA=%3
set GIT-SHA=%GIT-SHA:~0,6%
set SERVER-DEPLOY-FOLDER=deploy-%date%-%GIT-SHA%
cls

echo Creating Packages
echo.
call build /p:Configuration=Release-Api;Platform="Any CPU";DeployOnBuild=true;PublishProfile=Release-Api /flp1:logfile=masterbuild-errors.log;errorsonly;Verbosity=Normal /flp2:logfile=masterbuild-full.log;NoSummary;Verbosity=Normal /flp3:logfile=masterbuild-warnings.log;warningsonly;Verbosity=Normal
cls

echo Copying Release-Api-Pkg to %DEPLOY-FOLDER%
copy %sourceRoot%\deploy-apps.cmd %DEPLOY-FOLDER% >> master-copy.log
cls

call build /p:Configuration=Release-UI;Platform="Any CPU";DeployOnBuild=true;PublishProfile=Release-UI /flp1:logfile=masteruibuild-errors.log;errorsonly;Verbosity=Normal /flp2:logfile=masteruibuild-full.log;NoSummary;Verbosity=Normal /flp3:logfile=masteruibuild-warnings.log;warningsonly;Verbosity=Normal
cls

echo Copying Release-UI-Pkg to %DEPLOY-FOLDER%

rd %SERVER1-TEMP%\%SERVER-DEPLOY-FOLDER% /S /Q
cls
echo Copying Deployment to temp folder on Application server
robocopy %DEPLOY-FOLDER% %SERVER1-TEMP%\%SERVER-DEPLOY-FOLDER% /MIR > masterui-copy.log
cls

echo Packages created @ %cd%artifacts
echo.
echo For details on the build and package process check the logs for 
echo the various app types within the source root. Files are named
echo <APPTYPE>-<LOGTYPE>.log
echo.
echo.
echo DEPLOYING THESE PACKAGES
echo Each folder maps to a set of deployments for a particular server. 
echo.
echo To deploy copy the contents of the folder for the chosen server to 
echo C:\scm\temp\deploy-YYYYMMDD-SHA (Year, Month, Day) on the matching server 
echo via SMB or RDP. Once done RDP to the server and run the 
echo deploy-apps.cmd batch file as an Administrator. 
echo.
echo Note, deployment requires the shutdown of IIS services while the applications 
echo are updated. Be sure to take the system being updated out of the LB/failover
echo system for the duration of the update. 
echo.
echo REMEMBER: ALWAYS TEST THE APP AFTER DEPLOYMENT. AUTOMATION DOES NOT GAURENTEE PERFECTION. 
echo.
echo.
echo The execution of this script just saved you approx 1.5-3 hours.
echo.

%pkgStartDrive%
cd %pkgStartPath%


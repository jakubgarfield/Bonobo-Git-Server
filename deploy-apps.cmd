@echo off
cls
echo ========================================================================
echo ======================== Bonobo Git Installer  =========================
echo ========================================================================
echo This script will stop all IIS Services and Re-install/Upgrade all 
echo Bonobo.Git.Server Instances. 
echo.
echo Press Ctrl+C and answer Yes to Terminate Batch to exit now
echo.
pause
cls
cd %~dp0
net stop WAS /Y
echo =========== Installing BonoboGit Server - Api ===========
call Api.deploy.cmd /Y > Api.deploy.log
type Api.deploy.log
echo.
echo =========== Installing BonoboGit Server - UI (/manager) ===========
call ui.deploy.cmd /Y > ui.deploy.log
type ui.deploy.log
echo.
echo ========================================================================
echo ================ Installation complete. Restarting IIS =================
net start W3SVC
echo ========================= IIS Restart Complete =========================
echo ========================================================================
echo.
echo Installation is complete, applications are deployed and IIS should be responding to requests as normal.
echo Check the console log or Master.deploy.log and MasterUI.deploy.log for details.
echo.
pause

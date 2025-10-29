@echo off
powershell.exe -ExecutionPolicy Bypass -File "%~dp0\PostStageHook.ps1" -StageDirectory %1
exit %errorlevel%
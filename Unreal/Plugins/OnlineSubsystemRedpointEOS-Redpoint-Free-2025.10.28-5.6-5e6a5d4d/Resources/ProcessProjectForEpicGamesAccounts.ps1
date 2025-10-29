#
# This script is called by ProcessProjectAfterStaging.ps1 for wrapping the project with the EOS bootstrapper.
#
param(
    [string] $EngineDir, 
    [string] $ProjectDir, 
    [string] $PluginDir,
    [string] $StageDir,
    [string] $TargetPlatform,
    [string] $TargetConfiguration,
    [string] $ProductId,
    [string] $SandboxId,
    [string] $DeploymentId,
    [string] $SdkLocation,
    [string] $ExecutableName,
    [string] $FolderName,
    [string] $MainExeAbsolutePath,
    [string] $MainExeRelativePath,
    [bool] $CopyIconToBootstrapper
)

$ErrorActionPreference = "Stop"

function Set-RetryableContent([string] $Path, [string] $Value) {
    while ($true) {
        try {
            Set-Content -Force -Path $Path -Value $Value
            break
        } catch {
            if ($_.ToString().Contains("Stream was not readable.")) {
                continue
            }
        }
    }
}

# If this SDK version doesn't have the bootstrapper tool, then it doesn't need wrapping.
if (!(Test-Path "$SdkLocation\Tools\EOSBootstrapperTool.exe")) {
    exit 0
}

$CopyFlag = ""
if ($CopyIconToBootstrapper) {
    $CopyFlag = "-r"
}

try {
    # Copy bootstrapper tooling temporarily to the staging directory so that it can generate
    # the bootstrapper correctly.
    Copy-Item -Force "$SdkLocation\Tools\EOSBootstrapper.exe" "$StageDir\EOSBootstrapper.exe"
    Copy-Item -Force "$SdkLocation\Tools\EOSBootstrapperTool.exe" "$StageDir\EOSBootstrapperTool.exe"

    # Work in the staging directory for running the bootstrapper tool.
    Push-Location "$StageDir"

    # If this project was signed with Anti-Cheat, set up the bootstrapper slightly differently.
    if (Test-Path "$StageDir\InstallAntiCheat.bat") {
        # Move the launch executable back to start_protected_game.exe
        Move-Item -Force "$StageDir\$ExecutableName.exe" "$StageDir\start_protected_game.exe"

        # Use the bootstrapper tool to generate a new executable name.
        & "$StageDir\EOSBootstrapperTool.exe" `
            -o "$StageDir\$ExecutableName.exe" `
            -a "start_protected_game.exe" `
            -f $CopyFlag
    } else {
        # Remove launch executable that can't be used.
        if (Test-Path "$StageDir\$ExecutableName.exe") {
            Remove-Item -Force "$StageDir\$ExecutableName.exe"
        }

        # Use the bootstrapper tool to generate a new executable name.
        Push-Location "$StageDir"
        & "$StageDir\EOSBootstrapperTool.exe" `
            -o "$StageDir\$ExecutableName.exe" `
            -a "$MainExeRelativePath" `
            -f $CopyFlag
    }
} finally {
    Pop-Location
    if (Test-Path "$StageDir\EOSBootstrapper.exe") {
        Remove-Item -Force "$StageDir\EOSBootstrapper.exe"
    }
    if (Test-Path "$StageDir\EOSBootstrapperTool.exe") {
        Remove-Item -Force "$StageDir\EOSBootstrapperTool.exe"
    }
}

# Copy the EOS redistributable, which the developer needs to configure for install as part
# of their store distribution.
#
# NOTE: The presence of this file is also used by Anti-Cheat to determine whether it should wrap
#       the existing main launch file ($StageDir\$ExecutableName.exe) or forward directly
#       to $MainExeRelativePath.
#
Copy-Item "$SdkLocation\Tools\EpicOnlineServicesInstaller.exe" "$StageDir\EpicOnlineServicesInstaller.exe"

# Create batch scripts to install/uninstall EOS services.
Set-RetryableContent -Path "$StageDir\InstallEOSServices.bat" -Value @"
@echo off
cd %~dp0
EpicOnlineServicesInstaller.exe /install productId=$ProductId /quiet
"@
Set-RetryableContent -Path "$StageDir\UninstallEOSServices.bat" -Value @"
@echo off
cd %~dp0
EpicOnlineServicesInstaller.exe /uninstall productId=$ProductId /quiet
"@
param([Parameter(Mandatory=$false)][string] $ProjectDir)

$ErrorActionPreference = "Stop"

if ($PSScriptRoot.Contains("U5M")) {
    Write-Host "note: Skipping pre-build steps because the plugin is being built for distribution on Fab. This check detects 'U5M' in the full plugin folder path. If you are not Epic Games building the plugin for distribution on Fab, please rename the directory that your project or engine is located in."
    exit 0
}

foreach ($UPluginFile in (Get-ChildItem -Path $PSScriptRoot\.. -Filter *.uplugin)) {
    if ($UPluginFile.Name.StartsWith("OnlineSubsystemRedpointEOS_")) {
        Write-Host "error: Detected Redpoint EOS console support plugins are incorrectly merged into the main plugin folder. You must keep console plugins in their own folder (one folder per GitLab repository). For example, 'Plugins\EOS', 'Plugins\EOS-Platform1', 'Plugins\EOS-Platform2' and so forth. Refusing to build until you fix this."
        exit 1
    }
}

& "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" -ExecutionPolicy Bypass -File "$PSScriptRoot\ApplyCodeFileIds.ps1"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" -ExecutionPolicy Bypass -File "$PSScriptRoot\InstallStagingHooksInEngine.ps1" -ProjectDir $ProjectDir
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

exit 0
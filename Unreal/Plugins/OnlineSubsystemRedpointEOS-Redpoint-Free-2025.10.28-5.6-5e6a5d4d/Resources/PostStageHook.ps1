param([string]$StageDirectory)

$EngineDir = $env:uebp_LOCAL_ROOT
$ProjectDir = (Resolve-Path $PSScriptRoot\..\..).Path

# Try to locate the EOS Online Subsystem plugin.
$PluginDir = $null
if (Test-Path "$ProjectDir\Plugins") {
    foreach ($Item in (Get-ChildItem -Recurse -Filter OnlineSubsystemRedpointEOS.uplugin "$ProjectDir\Plugins")) {
        $PluginDir = $Item.Directory.FullName
        break
    }
}
if ((Test-Path "$EngineDir\Engine\Plugins\Marketplace") -and $PluginDir -eq $null) {
    foreach ($Item in (Get-ChildItem -Recurse -Filter OnlineSubsystemRedpointEOS.uplugin "$EngineDir\Engine\Plugins\Marketplace")) {
        $PluginDir = $Item.Directory.FullName
        break
    }
}
if ((Test-Path "$EngineDir\Engine\Plugins\Redpoint") -and $PluginDir -eq $null) {
    foreach ($Item in (Get-ChildItem -Recurse -Filter OnlineSubsystemRedpointEOS.uplugin "$EngineDir\Engine\Plugins\Redpoint")) {
        $PluginDir = $Item.Directory.FullName
        break
    }
}

if ($PluginDir -ne $null) {
    if (Test-Path "$PluginDir\Resources\ProcessProjectAfterStaging.ps1") {
        & "$PluginDir\Resources\ProcessProjectAfterStaging.ps1" -EngineDir "$EngineDir" -ProjectDir "$ProjectDir" -PluginDir "$PluginDir" -StageDir "$StageDirectory"
        exit $LASTEXITCODE
    }
}

exit 0
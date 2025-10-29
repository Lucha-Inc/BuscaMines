param($RootPath)

$ErrorActionPreference = "Stop"

function Process-File($Path) {
    $RawContent = (Get-Content -Path $Path -Raw)
    if ($RawContent -eq $null) {
        return 0
    }
    $GuardBeginPresent = $RawContent.Contains("REDPOINT_EOS_CODE_GUARD_BEGIN()")
    $GuardEndPresent = $RawContent.Contains("REDPOINT_EOS_CODE_GUARD_END()")
    $GuardBeginUObjectPresent = $RawContent.Contains("REDPOINT_EOS_CODE_GUARD_BEGIN_UOBJECT()")
    $GuardEndUObjectPresent = $RawContent.Contains("REDPOINT_EOS_CODE_GUARD_END_UOBJECT()")

    if ($GuardBeginPresent -and !$GuardEndPresent) {
        Write-Warning "Missing REDPOINT_EOS_CODE_GUARD_END(): $($Path.FullName)"
    }
    if ($GuardBeginUObjectPresent -and !$GuardEndUObjectPresent) {
        Write-Warning "Missing REDPOINT_EOS_CODE_GUARD_END(): $($Path.FullName)"
    }
    return 0
}

$SourceFiles = (Get-ChildItem -Recurse -Path $RootPath -Filter *.cpp)
$HeaderFiles = (Get-ChildItem -Recurse -Path $RootPath -Filter *.h)
foreach ($File in $SourceFiles) {
    $Result = (Process-File -Path ($File.FullName))
    if ($Result -ne 0) {
        exit $Result
    }
}
foreach ($File in $HeaderFiles) {
    $Result = (Process-File -Path ($File.FullName))
    if ($Result -ne 0) {
        exit $Result
    }
}

exit 0
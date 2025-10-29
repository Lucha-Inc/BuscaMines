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
    if (($GuardBeginPresent -and $GuardEndPresent) -or
        ($GuardBeginUObjectPresent -and $GuardEndUObjectPresent)) {
        return 0
    }

    $Lines = [System.Collections.ArrayList]::new((Get-Content -Path $Path))

    if (!$GuardEndPresent) {
        if ($Lines[$Lines.Count - 1].Trim() -ne "") {
            $Lines.Add("") | Out-Null
        }
        $Lines.Add("REDPOINT_EOS_CODE_GUARD_END()") | Out-Null
    }

    if (!$GuardBeginPresent) {
        $GuardBeginCreated = $false
        for ($i = 0; $i -lt $Lines.Count; $i++) {
            $Line = $Lines[$i].Trim();
            if ($Line.Trim() -eq "#include ""CoreMinimal.h""") {
                $Lines[$i] = "#include ""RedpointEOSBuild/BuildEnvironment.h"""
                continue
            } elseif ($Line -eq "") {
                continue;
            } elseif ($Line.StartsWith("#")) {
                continue;
            } elseif ($Line.StartsWith("//")) {
                continue;
            }

            # This is the first line of content.
            if ($i -eq 0 -or $Lines[$i-1].Trim() -ne "") {
                $Lines.Insert($i, "") | Out-Null
                $Lines.Insert($i+1, "REDPOINT_EOS_CODE_GUARD_BEGIN()") | Out-Null
                $Lines.Insert($i+2, "") | Out-Null
            } else {
                $Lines.Insert($i, "REDPOINT_EOS_CODE_GUARD_BEGIN()") | Out-Null
                $Lines.Insert($i+1, "") | Out-Null
            }
            $GuardBeginCreated = $true
            break
        }
        if (!$GuardBeginCreated) {
            Write-Host "Unable to find where to put REDPOINT_EOS_CODE_GUARD_BEGIN()"
            return 1
        }
    }

    Set-Content -Value ($Lines -join "`n") -Path $Path -NoNewline
    return 0
}

$SourceFiles = (Get-ChildItem -Recurse -Path $RootPath -Filter *.cpp)
$HeaderFiles = (Get-ChildItem -Recurse -Path $RootPath -Filter *.h)
foreach ($File in $SourceFiles) {
    Write-Host ($File.FullName)
    $Result = (Process-File -Path ($File.FullName))
    if ($Result -ne 0) {
        exit $Result
    }
}
foreach ($File in $HeaderFiles) {
    Write-Host ($File.FullName)
    $Result = (Process-File -Path ($File.FullName))
    if ($Result -ne 0) {
        exit $Result
    }
}

exit 0
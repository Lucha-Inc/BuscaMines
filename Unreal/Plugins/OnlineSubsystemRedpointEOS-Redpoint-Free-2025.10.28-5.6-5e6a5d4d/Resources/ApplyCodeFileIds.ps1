#
# This script updates plugin code to apply some pre-build transformations.
#
param()

$ErrorActionPreference = "Stop"

$FileNsIdRegex = [regex]"REDPOINT_EOS_FILE_NS_ID\((\s*)([`$A-Za-z0-9_:/]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*)\)"
$FileNsExportRegex = [regex]"REDPOINT_EOS_FILE_NS_EXPORT\((\s*)([`$A-Za-z0-9_:/]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*)\)"

$FileNsIdRegex_Old = [regex]"REDPOINT_EOS_FILE_NS_ID\((\s*)([`$A-Za-z0-9_:/]+)_Redpoint_([A-Za-z0-9_:]+)(\s*)\)"
$FileNsExportRegex_Old = [regex]"REDPOINT_EOS_FILE_NS_EXPORT\((\s*)([`$A-Za-z0-9_:/]+)_Redpoint_([A-Za-z0-9_:]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*)\)"
$FileNsIdRegex_Old2 = [regex]"REDPOINT_EOS_FILE_NS_ID\((\s*)([`$A-Za-z0-9_:/]+)_Redpoint::([A-Za-z0-9_:]+)(\s*)\)"
$FileNsExportRegex_Old2 = [regex]"REDPOINT_EOS_FILE_NS_EXPORT\((\s*)([`$A-Za-z0-9_:/]+)_Redpoint::([A-Za-z0-9_:]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*)\)"

$FileNsForwardDeclClassRegex = [regex]"REDPOINT_EOS_FILE_NS_FORWARD_DECLARE_CLASS\((\s*)([`$A-Za-z0-9_:/]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*)\)"
$FileNsForwardDeclStructRegex = [regex]"REDPOINT_EOS_FILE_NS_FORWARD_DECLARE_STRUCT\((\s*)([`$A-Za-z0-9_:/]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*)\)"
$FileNsForwardDeclTypedefRegex = [regex]"REDPOINT_EOS_FILE_NS_FORWARD_DECLARE_TYPEDEF\((\s*)([`$A-Za-z0-9_:/]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*)\)"
$FileNsForwardDeclEnumRegex = [regex]"REDPOINT_EOS_FILE_NS_FORWARD_DECLARE_ENUM\((\s*)([`$A-Za-z0-9_:/]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*,\s*)([A-Za-z0-9_:]+)(\s*)\)"

$AllDeclarations = @{}

function Process-File-Pass1($Path, $RootPath) {
    try {
        if ((Get-Item $Path).Name -eq "BuildEnvironment.h") {
            # Skip file which defines these macros.
            return 0
        }

        $RawContent = (Get-Content -Path $Path -Raw)
        if ($RawContent -eq $null) {
            # Skip file if we can't get the content.
            return 0
        }

        if (!$RawContent.Contains("REDPOINT_EOS_FILE_NS_ID") -and 
            !$RawContent.Contains("REDPOINT_EOS_FILE_NS_EXPORT")) {
            # Skip this file if it isn't using file namespaces.
            return 0
        }

        # Compute file ID.
        $NestedPath = $Path.Substring((Get-Item $RootPath).FullName.Length + 1)
        $NestedPath = $NestedPath.Substring(0, $NestedPath.Length - [System.IO.Path]::GetExtension($Path).Length)
        $NestedPath = $NestedPath.Replace('\', '/')
        $NestedPath = $NestedPath.Replace("/Public/", "/Hashed/").Replace("/Private/", "/Hashed/")
        $NestedPathBytes = [System.Text.Encoding]::UTF8.GetBytes($NestedPath)
        $Sha1 = New-Object System.Security.Cryptography.SHA1CryptoServiceProvider
        $Sha1ShortHash = ($Sha1.ComputeHash($NestedPathBytes) | Select-Object -First 4)
        $ResultHash = [BitConverter]::ToUInt32($Sha1ShortHash, 0).ToString()
        $Components = $NestedPath.Split('/')

        # Process export identifiers.
        $OriginalRawContent = $RawContent
        $RawContent = $FileNsIdRegex.Replace($RawContent, "REDPOINT_EOS_FILE_NS_ID(`${1}${ResultHash}`$3`$4`$5)")
        $RawContent = $FileNsExportRegex.Replace($RawContent, "REDPOINT_EOS_FILE_NS_EXPORT(`${1}${ResultHash}`$3`$4`$5`$6`$7)")
        $RawContent = $FileNsIdRegex_Old.Replace($RawContent, "REDPOINT_EOS_FILE_NS_ID(`${1}${ResultHash}, Redpoint::`$3`$4)")
        $RawContent = $FileNsExportRegex_Old.Replace($RawContent, "REDPOINT_EOS_FILE_NS_EXPORT(`${1}${ResultHash}, Redpoint::`$3`$4`$5`$6)")
        $RawContent = $FileNsIdRegex_Old2.Replace($RawContent, "REDPOINT_EOS_FILE_NS_ID(`${1}${ResultHash}, Redpoint::`$3`$4)")
        $RawContent = $FileNsExportRegex_Old2.Replace($RawContent, "REDPOINT_EOS_FILE_NS_EXPORT(`${1}${ResultHash}, Redpoint::`$3`$4`$5`$6)")

        # Record exports.
        foreach ($Match in $FileNsExportRegex.Matches($RawContent)) {
            $AllDeclarations["$($Match.Groups[4].Value)::$($Match.Groups[6].Value)"] = "${ResultHash}"
        }
        
        # Update the file if the content needs to change.
        if ($RawContent -ne $OriginalRawContent) {
            Write-Host "Syncing file IDs: $Path"
            Set-Content -Value $RawContent -Path $Path -NoNewline
        }
        return 0
    } catch {
        Write-Host $_
        return 1
    }
}

function Process-File-Pass2($Path) {
    try {
        if ((Get-Item $Path).Name -eq "BuildEnvironment.h") {
            # Skip file which defines these macros.
            return 0
        }

        $RawContent = (Get-Content -Path $Path -Raw)
        if ($RawContent -eq $null) {
            # Skip file if we can't get the content.
            return 0
        }

        if (!$RawContent.Contains("REDPOINT_EOS_FILE_NS_FORWARD_DECLARE_CLASS")) {
            # Skip this file if it isn't using forward declarations.
            return 0
        }

        # Iterate through forward declarations and update their file IDs.
        $OriginalRawContent = $RawContent
        $RawContent = $FileNsForwardDeclClassRegex.Replace($RawContent, {
            $_1 = $($args[0].Groups[1].Value)
            $_3 = $($args[0].Groups[3].Value)
            $_4 = $($args[0].Groups[4].Value)
            $_5 = $($args[0].Groups[5].Value)
            $_6 = $($args[0].Groups[6].Value)
            $_7 = $($args[0].Groups[7].Value)
            $Target = "$_4::$_6"
            $FileId = $AllDeclarations[$Target]
            if ($FileId -eq $null -or $FileId -eq "") {
                $FileId = "0"
            }
            return "REDPOINT_EOS_FILE_NS_FORWARD_DECLARE_CLASS(${_1}$FileId${_3}${_4}${_5}${_6}${_7})"
        })
        $RawContent = $FileNsForwardDeclStructRegex.Replace($RawContent, {
            $_1 = $($args[0].Groups[1].Value)
            $_3 = $($args[0].Groups[3].Value)
            $_4 = $($args[0].Groups[4].Value)
            $_5 = $($args[0].Groups[5].Value)
            $_6 = $($args[0].Groups[6].Value)
            $_7 = $($args[0].Groups[7].Value)
            $Target = "$_4::$_6"
            $FileId = $AllDeclarations[$Target]
            if ($FileId -eq $null -or $FileId -eq "") {
                $FileId = "0"
            }
            return "REDPOINT_EOS_FILE_NS_FORWARD_DECLARE_STRUCT(${_1}$FileId${_3}${_4}${_5}${_6}${_7})"
        })
        $RawContent = $FileNsForwardDeclTypedefRegex.Replace($RawContent, {
            $_1 = $($args[0].Groups[1].Value)
            $_3 = $($args[0].Groups[3].Value)
            $_4 = $($args[0].Groups[4].Value)
            $_5 = $($args[0].Groups[5].Value)
            $_6 = $($args[0].Groups[6].Value)
            $_7 = $($args[0].Groups[7].Value)
            $_8 = $($args[0].Groups[8].Value)
            $_9 = $($args[0].Groups[9].Value)
            $Target = "$_4::$_6"
            $FileId = $AllDeclarations[$Target]
            if ($FileId -eq $null -or $FileId -eq "") {
                $FileId = "0"
            }
            return "REDPOINT_EOS_FILE_NS_FORWARD_DECLARE_TYPEDEF(${_1}$FileId${_3}${_4}${_5}${_6}${_7}${_8}${_9})"
        })
        $RawContent = $FileNsForwardDeclEnumRegex.Replace($RawContent, {
            $_1 = $($args[0].Groups[1].Value)
            $_3 = $($args[0].Groups[3].Value)
            $_4 = $($args[0].Groups[4].Value)
            $_5 = $($args[0].Groups[5].Value)
            $_6 = $($args[0].Groups[6].Value)
            $_7 = $($args[0].Groups[7].Value)
            $Target = "$_4::$_6"
            $FileId = $AllDeclarations[$Target]
            if ($FileId -eq $null -or $FileId -eq "") {
                $FileId = "0"
            }
            return "REDPOINT_EOS_FILE_NS_FORWARD_DECLARE_ENUM(${_1}$FileId${_3}${_4}${_5}${_6}${_7})"
        })
        
        # Update the file if the content needs to change.
        if ($RawContent -ne $OriginalRawContent) {
            Write-Host "Syncing forward declarations: $Path"
            Set-Content -Value $RawContent -Path $Path -NoNewline
        }
        return 0
    } catch {
        Write-Host $_
        return 1
    }
}

$RootPaths = @(
    (Get-Item -Path "$PSScriptRoot\..\Source").FullName
)
foreach ($PluginDir in (Get-ChildItem -Path "$PSScriptRoot\..\..\.." -Filter "EOS-*")) {
    $PluginNestedDir = (Join-Path -Path $PluginDir.FullName -ChildPath "OnlineSubsystemRedpointEOS")
    if (Test-Path $PluginNestedDir) {
        $RootPaths += (Join-Path -Path $PluginNestedDir -ChildPath "Source")
    }
}

foreach ($RootPath in $RootPaths) {
    $AllDeclarations.Clear()
    $SourceFiles = (Get-ChildItem -Recurse -Path $RootPath -Filter *.cpp)
    $HeaderFiles = (Get-ChildItem -Recurse -Path $RootPath -Filter *.h)

    # Go through and sync file IDs on the declaration and export side.
    foreach ($File in $SourceFiles) {
        $Result = (Process-File-Pass1 -Path ($File.FullName) -RootPath $RootPath)
        if ($Result -ne 0) {
            exit $Result
        }
    }
    foreach ($File in $HeaderFiles) {
        $Result = (Process-File-Pass1 -Path ($File.FullName) -RootPath $RootPath)
        if ($Result -ne 0) {
            exit $Result
        }
    }

    # Go through and sync forward declarations now that we've collected all exports.
    foreach ($File in $SourceFiles) {
        $Result = (Process-File-Pass2 -Path ($File.FullName))
        if ($Result -ne 0) {
            exit $Result
        }
    }
    foreach ($File in $HeaderFiles) {
        $Result = (Process-File-Pass2 -Path ($File.FullName))
        if ($Result -ne 0) {
            exit $Result
        }
    }
}

exit 0
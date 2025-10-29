// Copyright June Rhodes. All Rights Reserved.

using EpicGames.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnrealBuildBase;
using UnrealBuildTool;

/// <summary>
/// Win64 platform support for EOS.
/// </summary>
public class RedpointEOSSDKPlatform_Win64 : IRedpointEOSSDKPlatform
{
    public bool IsActive(ReadOnlyTargetRules Target)
    {
        return Target.Platform == UnrealTargetPlatform.Win64;
    }

    public string GetPlatformSuffix()
    {
        return string.Empty;
    }

    public void ConfigureModule(string SdkBase, string ModuleBase, ModuleRules Module, ReadOnlyTargetRules Target)
    {
        Module.PublicIncludePaths.Add(Path.Combine(SdkBase, "Include"));
        Module.PublicAdditionalLibraries.Add(Path.Combine(SdkBase, "Lib", "EOSSDK-Win64-Shipping.lib"));
        Module.RuntimeDependencies.Add("$(TargetOutputDir)/RedpointEOS/EOSSDK-Win64-Shipping.dll", Path.Combine(SdkBase, "Bin", "EOSSDK-Win64-Shipping.dll"));
        // @note: This file is now required at runtime on Win64 by the EOS SDK, and can not be made optional.
        Module.RuntimeDependencies.Add("$(TargetOutputDir)/RedpointEOS/x64/xaudio2_9redist.dll", Path.Combine(SdkBase, "Bin", "x64", "xaudio2_9redist.dll"));
        Module.PublicDelayLoadDLLs.Add("EOSSDK-Win64-Shipping.dll");
        Module.ExternalDependencies.Add(Path.Combine(SdkBase, "Include", "eos_version.h"));

        RedpointEOSSDK.EnsureDirectoryExists(new[] {
            Path.Combine(SdkBase, "Include"),
        });
        RedpointEOSSDK.EnsureFilesExist(new[] {
            Path.Combine(SdkBase, "Include", "eos_version.h"),
            Path.Combine(SdkBase, "Lib", "EOSSDK-Win64-Shipping.lib"),
            Path.Combine(SdkBase, "Bin", "EOSSDK-Win64-Shipping.dll"),
            Path.Combine(SdkBase, "Bin", "x64", "xaudio2_9redist.dll")
        });
    }

    public RedpointEOSSDKVersionConstraints GetSDKVersionConstraints()
    {
        return RedpointEOSSDKCommonConstraints.GetDesktopSDKVersionConstraints();
    }
}
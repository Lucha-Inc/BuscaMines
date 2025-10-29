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
/// Linux ARM64 platform support for EOS.
/// </summary>
public class RedpointEOSSDKPlatform_LinuxArm64 : IRedpointEOSSDKPlatform
{
    public bool IsActive(ReadOnlyTargetRules Target)
    {
        return Target.Platform == UnrealTargetPlatform.LinuxArm64;
    }

    public string GetPlatformSuffix()
    {
        return string.Empty;
    }

    public void ConfigureModule(string SdkBase, string ModuleBase, ModuleRules Module, ReadOnlyTargetRules Target)
    {
        Module.PublicIncludePaths.Add(Path.Combine(SdkBase, "Include"));
        Module.PublicAdditionalLibraries.Add(Path.Combine(SdkBase, "Bin", "libEOSSDK-LinuxArm64-Shipping.so"));
        Module.RuntimeDependencies.Add("$(TargetOutputDir)/libEOSSDK-LinuxArm64-Shipping.so", Path.Combine(SdkBase, "Bin", "libEOSSDK-LinuxArm64-Shipping.so"));
        Module.ExternalDependencies.Add(Path.Combine(SdkBase, "Include", "eos_version.h"));

        if (Target.Type != TargetType.Server)
        {
            Console.WriteLine("warning: LinuxArm64 target is only supported for dedicated server targets.");
        }
        
        RedpointEOSSDK.EnsureDirectoryExists(new[] {
            Path.Combine(SdkBase, "Include"),
        });
        RedpointEOSSDK.EnsureFilesExist(new[] {
            Path.Combine(SdkBase, "Include", "eos_version.h"),
            Path.Combine(SdkBase, "Bin", "libEOSSDK-LinuxArm64-Shipping.so")
        });
    }

    public RedpointEOSSDKVersionConstraints GetSDKVersionConstraints()
    {
        return RedpointEOSSDKCommonConstraints.GetDesktopSDKVersionConstraints();
    }
}
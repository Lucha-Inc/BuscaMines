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
/// macOS platform support for EOS.
/// </summary>
public class RedpointEOSSDKPlatform_Mac : IRedpointEOSSDKPlatform
{
    public bool IsActive(ReadOnlyTargetRules Target)
    {
        return Target.Platform == UnrealTargetPlatform.Mac;
    }

    public string GetPlatformSuffix()
    {
        return string.Empty;
    }

    public void ConfigureModule(string SdkBase, string ModuleBase, ModuleRules Module, ReadOnlyTargetRules Target)
    {
        var BinaryPath = Path.Combine(SdkBase, "Bin");
        if (!File.Exists(Path.Combine(BinaryPath, "libEOSSDK-Mac-Shipping.dylib")) &&
            !Path.GetRelativePath(Path.GetFullPath(Module.EngineDirectory), Path.GetFullPath(SdkBase)).StartsWith(".."))
        {
            // We are using the EOS SDK from the engine. For some reason, the .dylib is not placed in the correct directory
            // for the EOS SDK shipped with the engine. Adjust the path for the binary file.
            BinaryPath = Path.Combine(Module.EngineDirectory, "Binaries", "ThirdParty", "EOSSDK", "Mac");
        }

        Module.PublicIncludePaths.Add(Path.Combine(SdkBase, "Include"));
        // UBT adds the source path of the runtime dependency to the RPATH, not the destination path where the 
        // file will be located. When we use $(TargetOutputDir) on macOS, packaged games will crash because 
        // the /Contents/UE4/<project>/Binaries/Mac/ path doesn't get added as an RPATH.
        //
        // So instead, we have to put the library in a location that will already be on the RPATH. 
        // $(BinaryOutputDir) resolves to the /Contents/MacOS/ directory, where the main executable file is, 
        // and this is also on the RPATH.
        //
        // Note that this change also impacts EOSDynamicLoaderDefault. Refer to the comment on line 78 of
        // EOSDynamicLoaderDefault.cpp to see how the loader accesses this alternate path.
        if (Environment.GetEnvironmentVariable("BUILDING_FOR_REDISTRIBUTION") != "true" ||
            // @note: This should ensure that our boot tests copy correctly even though the
            // BUILDING_FOR_REDISTRIBUTION will have been set.
            Target.Name.StartsWith("Project"))
        {
            Module.RuntimeDependencies.Add("$(BinaryOutputDir)/libEOSSDK-Mac-Shipping.dylib", Path.Combine(BinaryPath, "libEOSSDK-Mac-Shipping.dylib"));
        }
        else
        {
            // If the environment variable BUILDING_FOR_REDISTRIBUTION is set to true, then we are building the plugin for
            // redistribution (Free Edition). When building the plugin, $(BinaryOutputDir) maps to a folder within the
            // plugin's folder. However, building the plugin for redistribution doesn't cause the EOS SDK to be copied here.
            //
            // In the past, we used BuildPlugin, which didn't care if the output artifacts of the build didn't exist. We now
            // use BuildGraph however, and it *does* check to make sure all the output artifacts exist. This causes the build to
            // fail if we're using $(BinaryOutputDir).
            //
            // However, in these cases we know that we're building the Free Edition and that the developer will need to provide
            // their own copy of the EOS SDK (which will cause the above branch to run and have the EOS SDK copied out to
            // BinaryOutputDir correctly). So use this workaround so that the build will pass under BuildGraph.
            Module.RuntimeDependencies.Add("$(TargetOutputDir)/libEOSSDK-Mac-Shipping.dylib", Path.Combine(BinaryPath, "libEOSSDK-Mac-Shipping.dylib"));
        }
        Module.PublicDelayLoadDLLs.Add(Path.Combine(BinaryPath, "libEOSSDK-Mac-Shipping.dylib"));
        Module.ExternalDependencies.Add(Path.Combine(SdkBase, "Include", "eos_version.h"));

        // Attempt to remove the quarantine bit off the .dylib file, to prevent issues when it gets included in the packaged game.
        RedpointEOSSDK.StripQuarantineBit(Path.Combine(BinaryPath, "libEOSSDK-Mac-Shipping.dylib"));

        // If the user has EAS credentials installed for unit tests, copy across the JSON file so that the test can
        // execute on device.
        var easCredentialsPath = RedpointEOSSDK.LocateEASAutomatedTestingCredentialsPath(Target);
        if (easCredentialsPath != null)
        {
            Module.RuntimeDependencies.Add("$(TargetOutputDir)/Credentials.json", easCredentialsPath);
        }

        RedpointEOSSDK.EnsureDirectoryExists(new[] {
            Path.Combine(SdkBase, "Include"),
        });
        RedpointEOSSDK.EnsureFilesExist(new[] {
            Path.Combine(SdkBase, "Include", "eos_version.h"),
            Path.Combine(BinaryPath, "libEOSSDK-Mac-Shipping.dylib")
        });
    }

    public RedpointEOSSDKVersionConstraints GetSDKVersionConstraints()
    {
        return RedpointEOSSDKCommonConstraints.GetDesktopSDKVersionConstraints();
    }
}
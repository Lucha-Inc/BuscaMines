// Copyright June Rhodes. All Rights Reserved.

using System;
using System.IO;
using UnrealBuildTool;

public class RedpointEOSPlatformIntegrationMeta : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformIntegrationMeta(ReadOnlyTargetRules Target) : base("REPIO", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
        });

        // This module only supports Windows and Android.
        if (Target.Platform != UnrealTargetPlatform.Win64 &&
            Target.Platform != UnrealTargetPlatform.Android)
        {
            PrivateDefinitions.Add("REDPOINT_EOS_META_ENABLED=0");
            return;
        }

#if UE_5_5_OR_LATER
		// Turn off this module if not desired by the build.
		if (!RedpointEOSBuild.GetConfigBool(Target, "ENABLE_OCULUS", false) &&
			!RedpointEOSBuild.GetConfigBool(Target, "ENABLE_META", false) &&
			Environment.GetEnvironmentVariable("REDPOINT_STRICT_BUILD") != "1")
		{
			PrivateDefinitions.Add("REDPOINT_EOS_META_ENABLED=0");
			return;
		}

		// Try to find the OVRPlatformSDK.
		var SdkBase = LocateSdkDirectory(this, Target, ModuleDirectory);
		if (SdkBase == null)
		{
			Console.WriteLine("error: Meta Quest support was enabled with the ONLINE_SUBSYSTEM_EOS_ENABLE_META=1 ProjectDefinition, but this machine does not have the OVR Platform SDK installed. Unreal Engine 5.5 no longer ships the OVR Platform SDK, so you need to download it separately from https://developers.meta.com/horizon/downloads/package/oculus-platform-sdk and install it into C:\\ProgramData\\OVRPlatformSDK.");
			throw new Exception("Redpoint EOS Meta Quest support is missing one or more required files.");
		}

		PrivateIncludePaths.Add(Path.Combine(SdkBase, "Include"));

		if (Target.Platform == UnrealTargetPlatform.Win64)
		{
			if (File.Exists(Path.Combine(SdkBase, "Windows", "LibOVRPlatformImpl64_1.lib")))
			{
				PublicAdditionalLibraries.Add(Path.Combine(SdkBase, "Windows", "LibOVRPlatformImpl64_1.lib"));
				PublicDelayLoadDLLs.Add("LibOVRPlatformImpl64_1.dll");
				PrivateIncludePaths.Add(Path.Combine(SdkBase, "Windows"));

				PrivateDefinitions.Add("REDPOINT_EOS_META_USE_REDPOINT_PLATFORM_LOADER=0");
			}
			else
			{
				PublicAdditionalLibraries.Add(Path.Combine(SdkBase, "Windows", "LibOVRPlatform64_1.lib"));
				PublicDelayLoadDLLs.Add("LibOVRPlatform64_1.dll");

				PrivateDefinitions.Add("REDPOINT_EOS_META_USE_REDPOINT_PLATFORM_LOADER=1");
			}
		}
		else
		{
			// This uses the same Gradle/UPL trick as documented in RedpointEOSSDKPlatform_Android.Build.cs.
			RuntimeDependencies.Add(
				"$(TargetOutputDir)/arm64-v8a-libovrplatformloader.so",
				Path.Combine(SdkBase, "Android", "libs", "arm64-v8a", "libovrplatformloader.so"));
			RuntimeDependencies.Add(
				"$(TargetOutputDir)/armeabi-v7a-libovrplatformloader.so",
				Path.Combine(SdkBase, "Android", "libs", "armeabi-v7a", "libovrplatformloader.so"));

			PublicAdditionalLibraries.Add(Path.Combine(SdkBase, "Android", "libs", "arm64-v8a", "libovrplatformloader.so"));
			AdditionalPropertiesForReceipt.Add("AndroidPlugin", Path.Combine(ModuleDirectory, "RedpointEOSPlatformIntegrationMeta_Android_UPL.xml"));

			PrivateDefinitions.Add("REDPOINT_EOS_META_USE_REDPOINT_PLATFORM_LOADER=0");
		}
#else
        // Turn off this module if not desired by the build.
        if (!RedpointEOSBuild.GetConfigBool(Target, "ENABLE_OCULUS", true) &&
            !RedpointEOSBuild.GetConfigBool(Target, "ENABLE_META", true) &&
            Environment.GetEnvironmentVariable("REDPOINT_STRICT_BUILD") != "1")
        {
            PrivateDefinitions.Add("REDPOINT_EOS_META_ENABLED=0");
            return;
        }

        // We only use OnlineSubsystem when trying to load Oculus online subsystem
        // on earlier engine versions.
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "OnlineSubsystem",
            "OnlineSubsystemUtils",
            "LibOVRPlatform",
        });

        if (Target.Platform == UnrealTargetPlatform.Win64)
        {
            // Fix LibOVRPlatform not pulling in runtime dependencies on Windows.
            PublicDelayLoadDLLs.Add("LibOVRPlatform64_1.dll");
        }

        PrivateDefinitions.Add("REDPOINT_EOS_META_USE_REDPOINT_PLATFORM_LOADER=1");
#endif

        PrivateDefinitions.Add("REDPOINT_EOS_META_ENABLED=1");

        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
            "Json",
            "RedpointEOSAPI",
            "RedpointEOSAuth",
            "RedpointEOSCore",
            "RedpointEOSIdentity",
            "RedpointEOSPlatform",
            "RedpointEOSPresence",
            "RedpointEOSRooms",
            "RedpointEOSUserCache",
        });
    }

    private static string LocateSdkDirectory(ModuleRules Module, ReadOnlyTargetRules Target, string ModuleBase)
    {
        var CandidateDirectories = new[]
        {
			// Where the ZIP has been extracted to "ThirdParty-OVRPlatformSDK" and placed next to "RedpointEOSPlatformIntegrationMeta".
			Path.Combine(Path.GetFullPath(ModuleBase), "..", "ThirdParty-OVRPlatformSDK"),
			// Where the ZIP has been extracted into C:\ProgramData\OVRPlatformSDK\
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "OVRPlatformSDK"),
			// Where the ZIP has been extracted into the user's downloads folder
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "ovr_platform_sdk_69.0"),
			// Where the ZIP has been extracted into C:\OVRPlatformSDK\
			Path.Combine(Environment.GetEnvironmentVariable("SYSTEMDRIVE"), "OVRPlatformSDK"),
        };

        foreach (var Dir in CandidateDirectories)
        {
            if (Directory.Exists(Dir))
            {
                Console.WriteLine("EOSSDK (Meta Quest): Located OVR Platform SDK at: " + Dir);
                return Dir;
            }
        }

        return null;
    }
}
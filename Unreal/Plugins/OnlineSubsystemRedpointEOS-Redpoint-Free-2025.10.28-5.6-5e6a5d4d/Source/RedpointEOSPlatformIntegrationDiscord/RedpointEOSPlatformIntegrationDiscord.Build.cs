// Copyright June Rhodes. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using UnrealBuildTool;

public class RedpointEOSPlatformIntegrationDiscord : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSPlatformIntegrationDiscord(ReadOnlyTargetRules Target) : base("REPID", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
        });

        // This module currently only supports desktop platforms.
        if (Target.Platform != UnrealTargetPlatform.Win64 &&
            Target.Platform != UnrealTargetPlatform.Mac &&
            Target.Platform != UnrealTargetPlatform.Linux)
        {
            PrivateDefinitions.Add("REDPOINT_EOS_DISCORD_ENABLED=0");
            return;
        }

        // This module does not support server-only targets.
        if (Target.Type == TargetType.Program ||
            Target.Type == TargetType.Server)
        {
            PrivateDefinitions.Add("REDPOINT_EOS_DISCORD_ENABLED=0");
            return;
        }

        // Turn off this module if not desired by the build.
        if (!RedpointEOSBuild.GetConfigBool(Target, "ENABLE_DISCORD", false) &&
            Environment.GetEnvironmentVariable("REDPOINT_STRICT_BUILD") != "1")
        {
            PrivateDefinitions.Add("REDPOINT_EOS_DISCORD_ENABLED=0");
            return;
        }

        // Try to find the Discord Social SDK.
        List<string> SdkErrorLogs = null;
        var SdkBase = RedpointSDKLocator.LocateSdkDirectory(
            this,
            Target,
            "DiscordSocialSDK",
            string.Empty,
            new RedpointEOSSDKVersionConstraints(),
            out _,
            ref SdkErrorLogs);
        if (SdkBase == null)
        {
            Console.WriteLine("error: Discord support was enabled with the ONLINE_SUBSYSTEM_EOS_ENABLE_DISCORD=1 ProjectDefinition, but this machine does not have the Discord Social SDK installed. Please download it from the Discord developer portal and install it into C:\\ProgramData\\DiscordSocialSdk.");
            throw new Exception("Redpoint EOS Discord support is missing one or more required files.");
        }

        PrivateIncludePaths.Add(Path.Combine(SdkBase, "include"));

        if (Target.Platform == UnrealTargetPlatform.Win64)
        {
            PublicAdditionalLibraries.Add(Path.Combine(SdkBase, "lib", "release", "discord_partner_sdk.lib"));
            RuntimeDependencies.Add("$(TargetOutputDir)/discord_partner_sdk.dll", Path.Combine(SdkBase, "bin", "release", "discord_partner_sdk.dll"));
            PublicDelayLoadDLLs.Add("discord_partner_sdk.dll");
        }
        else if (Target.Platform == UnrealTargetPlatform.Mac)
        {
            if (Environment.GetEnvironmentVariable("BUILDING_FOR_REDISTRIBUTION") != "true" ||
                Target.Name.StartsWith("Project"))
            {
                RuntimeDependencies.Add("$(BinaryOutputDir)/libdiscord_partner_sdk.dylib", Path.Combine(SdkBase, "lib", "release", "libdiscord_partner_sdk.dylib"));
            }
            else
            {
                RuntimeDependencies.Add("$(TargetOutputDir)/libdiscord_partner_sdk.dylib", Path.Combine(SdkBase, "lib", "release", "libdiscord_partner_sdk.dylib"));
            }
            PublicDelayLoadDLLs.Add(Path.Combine(SdkBase, "lib", "release", "libdiscord_partner_sdk.dylib"));
            RedpointEOSSDK.StripQuarantineBit(Path.Combine(SdkBase, "lib", "release", "libdiscord_partner_sdk.dylib"));
        }
        else if (Target.Platform == UnrealTargetPlatform.Linux)
        {
            RuntimeDependencies.Add("$(TargetOutputDir)/libdiscord_partner_sdk.so", Path.Combine(SdkBase, "lib", "release", "libdiscord_partner_sdk.so"));
            PublicAdditionalLibraries.Add(Path.Combine(SdkBase, "lib", "release", "libdiscord_partner_sdk.so"));
        }
        else
        {
            throw new Exception("Platform not supported for Discord Social SDK!");
        }

        PrivateDependencyModuleNames.AddRange(new[]
        {
            "RedpointEOSAPI",
            "RedpointEOSAuth",
            "RedpointEOSCore",
            "RedpointEOSIdentity",
            "RedpointEOSPlatform",
            "RedpointEOSPresence",
            "RedpointEOSRooms",
        });

        PrivateDefinitions.Add("REDPOINT_EOS_DISCORD_ENABLED=1");
    }
}
// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSAuth : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSAuth(ReadOnlyTargetRules Target) : base("REAU", Target)
    {
        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
            "RedpointEOSAPI",
            "RedpointEOSCompat",
            "RedpointEOSConfig",
            "RedpointEOSCore",
            "RedpointEOSSDK",
            "RedpointEOSPlatform",
            "UMG",
            "HTTP",
            "Json",
            "Slate",
            "SlateCore",

			// @note: Only while we still have nodes that use OSSv1 to perform login with the local platform.
			"OnlineSubsystem",
            "OnlineSubsystemUtils",
        });

        // This is necessary so that the Default authentication graph can skip loading
        // the Oculus subsystem if we're running an Android application that wasn't
        // packaged for it.
        if (Target.Platform == UnrealTargetPlatform.Android)
        {
            PublicDependencyModuleNames.Add("Launch");
        }
    }
}
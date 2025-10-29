// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class OnlineSubsystemRedpointEOS : RedpointEOSCPlusPlusModuleRules
{
    public OnlineSubsystemRedpointEOS(ReadOnlyTargetRules Target) : base("OSRE", Target)
    {
        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "OnlineSubsystemUtils",
            "Projects",
            "Json",
            "VoiceChat",

            "RedpointEOSAPI",
            "RedpointEOSAuth",
            "RedpointEOSConfig",
            "RedpointEOSCore",
            "RedpointEOSRooms",
            "RedpointEOSPlatform",
            "RedpointEOSUserCache",
            "RedpointEOSStorage",

            // Now necessary for the EOSError.h header in Public.
            "OnlineSubsystem",

            // Now necessary as UEOSSubsystem is a UObject which depends on UUserWidget (and because it is a UObject, it can't be excluded for server-only builds).
            "UMG",
        });

        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",

            "RedpointEOSAntiCheat",
            "RedpointEOSAsync",
            "RedpointEOSCompat",
            "RedpointEOSFriends",
            "RedpointEOSIdentity",
            "RedpointEOSInterfaces",
            "RedpointEOSLicenseValidation",
            "RedpointEOSNetworking",
            "RedpointEOSOrchestrator",
            "RedpointEOSPresence",
            "RedpointEOSSDK",

			// Now necessary for the Cloud Messaging Hub code.
			"WebSockets",
        });

        // Prevent implicit narrowing of types on Windows. We have to use these macros around any 
        // external headers because the engine is not compliant with these requirements, and turning 
        // them on globally causes the engine headers to fail the build.
        if (Target.Platform == UnrealTargetPlatform.Win64)
        {
            PublicDefinitions.Add("EOS_ENABLE_STRICT_WARNINGS=__pragma(warning(push))__pragma(warning(error:4244))__pragma(warning(error:4838))");
            PublicDefinitions.Add("EOS_DISABLE_STRICT_WARNINGS=__pragma(warning(pop))");
        }
        else
        {
            PublicDefinitions.Add("EOS_ENABLE_STRICT_WARNINGS=");
            PublicDefinitions.Add("EOS_DISABLE_STRICT_WARNINGS=");
        }

    }
}
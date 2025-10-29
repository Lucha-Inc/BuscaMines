// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSIdentity : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSIdentity(ReadOnlyTargetRules Target) : base("REID", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
            "RedpointEOSAPI",
            "RedpointEOSCore",
            "RedpointEOSCompat",
            "RedpointEOSAuth",
            "RedpointEOSUserCache",
        });

        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "Engine",
            "CoreUObject",
            "RedpointEOSSDK",
            "RedpointEOSPlatform",
        });

        // @note: This can be set to 1 to enforce a 10 second timeout on identity hooks in builds that have logs enabled.
        PrivateDefinitions.Add("REDPOINT_EOS_ENABLE_IDENTITY_HOOK_TIMEOUT=0");
    }
}
// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointLibHydrogen : RedpointEOSCPlusPlusModuleRules
{
    public RedpointLibHydrogen(ReadOnlyTargetRules Target) : base("RLH", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "OpenSSL",
        });
    }
}
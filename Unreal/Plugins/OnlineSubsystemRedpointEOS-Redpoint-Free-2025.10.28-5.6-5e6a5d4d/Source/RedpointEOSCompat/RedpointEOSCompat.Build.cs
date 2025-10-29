// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSCompat : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSCompat(ReadOnlyTargetRules Target) : base("RECMP", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
        });
    }
}
// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSAsync : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSAsync(ReadOnlyTargetRules Target) : base("REAY", Target)
    {
        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "ApplicationCore",
        });

        PrivateDependencyModuleNames.Add("CoreUObject");
    }
}
// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSOrchestrator : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSOrchestrator(ReadOnlyTargetRules Target) : base("REO", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
            "RedpointEOSAPI",
            "RedpointEOSAsync",
        });

        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "CoreUObject",
            "Engine",
            "Json",
            "HTTP",
        });

        if (Target.Type == TargetType.Editor || Target.Type == TargetType.Server)
        {
            PrivateDefinitions.Add("REDPOINT_EOS_ORCHESTRATORS_ENABLED=1");
        }
        else
        {
            PrivateDefinitions.Add("REDPOINT_EOS_ORCHESTRATORS_ENABLED=0");
        }
    }
}
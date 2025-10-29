// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSConfigInspector : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSConfigInspector(ReadOnlyTargetRules Target) : base("RECI", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "UnrealEd",
            "Slate",
            "SlateCore",
            "EditorStyle",
            "WorkspaceMenuStructure",
            "InputCore",
            "CoreUObject",
        });
    }
}
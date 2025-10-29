// Copyright June Rhodes. All Rights Reserved.

using System;
using UnrealBuildTool;

public class RedpointEOSEditor : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSEditor(ReadOnlyTargetRules Target) : base("REE", Target)
    {
        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "Projects",
            "InputCore",
            "UnrealEd",
            "ToolMenus",
            "CoreUObject",
            "Engine",
            "Slate",
            "SlateCore",
            "Settings",
            "MainFrame",
            "HTTP",
            "OnlineSubsystem",
            "OnlineSubsystemUtils",
            "OnlineSubsystemRedpointEOS",
            "RedpointEOSAuth",
            "RedpointEOSAPI",
            "RedpointEOSSDK",
            "RedpointEOSCore",
            "RedpointEOSLicenseValidation",
            "RedpointEOSShared",
            "EditorStyle",
            "RedpointLibHydrogen",
            "DesktopPlatform",
            "DeveloperSettings",
            "SourceControl",
            "RedpointEOSConfig",
            "InterchangeCore",
            "libzip",
            "TargetDeviceServices",
            "BlueprintGraph",
            "OnlineSubsystemBlueprints",
            "XmlParser",
        });
    }
}
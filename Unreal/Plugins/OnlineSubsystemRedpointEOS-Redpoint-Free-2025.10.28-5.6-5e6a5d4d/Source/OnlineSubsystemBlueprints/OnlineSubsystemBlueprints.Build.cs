// Copyright June Rhodes. All Rights Reserved.

using EpicGames.Core;
using System;
using System.IO;
using UnrealBuildTool;

public class OnlineSubsystemBlueprints : RedpointEOSCPlusPlusModuleRules
{
    public OnlineSubsystemBlueprints(ReadOnlyTargetRules Target) : base("OnlineSubsystemBlueprints", Target)
    {
        PublicDependencyModuleNames.AddRange(
            new string[]
            {
                "Core",
                "OnlineSubsystem",
                "OnlineSubsystemUtils",
                "RedpointEOSInterfaces",
                "VoiceChat",
            }
        );

        PrivateDependencyModuleNames.AddRange(
            new string[]
            {
                "CoreUObject",
                "Engine",
                "Json",
                "CoreOnline",
            }
        );

        if (!bIsNoEOS)
        {
            PrivateDependencyModuleNames.Add("RedpointEOSCore");
            PrivateDefinitions.Add("REDPOINT_OSB_WITH_EOS=1");
        }
        else
        {
            PrivateDefinitions.Add("REDPOINT_OSB_WITH_EOS=0");
        }
    }
}
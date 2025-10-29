// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSCore : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSCore(ReadOnlyTargetRules Target) : base("REC", Target)
    {
        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "CoreOnline",
            "Engine",
            "RedpointEOSSDK",
            "RedpointEOSBuild",
            "RedpointEOSAPI",
            "RedpointEOSConfig",
            "RedpointEOSPlatform",
        });

        PrivateDependencyModuleNames.AddRange(new[]
        {
            "Json",
            "Projects",
            "HTTP",
        });

        if (Target.Type != TargetType.Server)
        {
            PublicDefinitions.Add("REDPOINT_EOS_HAS_IMAGE_DECODING=1");

            PrivateDependencyModuleNames.AddRange(new[]
            {
                // Required for image decoding.
                "ImageWrapper",
                "RHI",
                "RenderCore",
            });
        }
        else
        {
            PublicDefinitions.Add("REDPOINT_EOS_HAS_IMAGE_DECODING=0");
        }
    }
}
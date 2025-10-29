// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSAPI : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSAPI(ReadOnlyTargetRules Target) : base("REA", Target)
    {
        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "RedpointEOSAsync",
            "RedpointEOSSDK",
            "ApplicationCore",
        });

        PrivateDependencyModuleNames.Add("GameplayDebugger");

        PrivateDefinitions.Add("REDPOINTEOSAPI_PACKAGE=1");

        // Polyfill the WITH_CLIENT_CODE define on Unreal Engine 5.4 and earlier.
#if !UE_5_5_OR_LATER
        var bWithClientCode = Target.Type == TargetType.Game
            || Target.Type == TargetType.Client
            || Target.Type == TargetType.Editor;
        PublicDefinitions.Add(string.Format("WITH_CLIENT_CODE={0}", bWithClientCode ? 1 : 0));
#endif
    }
}
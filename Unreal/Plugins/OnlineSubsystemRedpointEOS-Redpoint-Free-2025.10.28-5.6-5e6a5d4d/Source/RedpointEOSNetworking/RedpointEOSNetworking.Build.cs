// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;

public class RedpointEOSNetworking : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSNetworking(ReadOnlyTargetRules Target) : base("REN", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreOnline",
            "RedpointEOSAPI",
        });

        PrivateDependencyModuleNames.AddRange(new string[]
        {
            "AESGCMHandlerComponent",
            "CoreUObject",
            "Engine",
            "GameplayDebugger",
            "RedpointEOSAntiCheat",
            "RedpointEOSCompat",
            "RedpointEOSCore",
            "RedpointEOSIdentity",
            "RedpointLibHydrogen",
            "Sockets",
            "NetCore",
            "PacketHandler",

            // Contains the beacon actors. We don't rely on OSSv1 otherwise.
            "OnlineSubsystemUtils",
        });

        // Turn this on if you want socket-level network tracing emitted
        // to the logs. This data is *extremely* verbose and should not be used
        // unless you're trying to figure out some hard-to-diagnose networking
        // issue.
        if (RedpointEOSBuild.GetConfigBool(Target, "ENABLE_NETWORK_TRACING", false))
        {
            PrivateDefinitions.Add("REDPOINT_EOS_ENABLE_SOCKET_LEVEL_NETWORK_TRACING=1");
        }
        else
        {
            PrivateDefinitions.Add("REDPOINT_EOS_ENABLE_SOCKET_LEVEL_NETWORK_TRACING=0");
        }

        SetupIrisSupport(Target, false);
    }
}
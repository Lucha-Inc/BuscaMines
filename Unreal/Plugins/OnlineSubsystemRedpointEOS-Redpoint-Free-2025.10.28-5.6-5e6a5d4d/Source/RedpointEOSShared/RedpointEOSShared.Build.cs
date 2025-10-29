// Copyright June Rhodes. All Rights Reserved.

using System;
using System.IO;
using UnrealBuildTool;

public class RedpointEOSShared : RedpointEOSCPlusPlusModuleRules
{
    public RedpointEOSShared(ReadOnlyTargetRules Target) : base("RESH", Target)
    {
        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
        });

        // Only Unreal Engine 5.5 and later have the critical "don't register the Epic EOS SDK Manager
        // if one is already registered" logic, so this module is completely turned off on earlier
        // engine versions.
#if UE_5_5_OR_LATER
		PublicDependencyModuleNames.AddRange(new string[]
		{
			"RedpointEOSSDK",
		});

		PrivateDependencyModuleNames.AddRange(new string[]
		{
			"RedpointEOSAPI",
		});
#endif
    }
}
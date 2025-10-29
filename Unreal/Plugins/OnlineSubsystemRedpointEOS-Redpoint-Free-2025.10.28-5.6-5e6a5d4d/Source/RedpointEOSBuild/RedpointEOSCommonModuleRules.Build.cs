// Copyright June Rhodes. All Rights Reserved.

using EpicGames.Core;
using System;
using UnrealBuildBase;
using UnrealBuildTool;

/// <summary>
/// Provides the root implementation of ModuleRules used by the more specific
/// RedpointEOSCPlusPlusModuleRules and RedpointEOSExternalModuleRules classes.
/// </summary>
public abstract class RedpointEOSCommonModuleRules : ModuleRules
{
    static bool bEmittedStrictModeMessage = false;

    protected RedpointEOSCommonModuleRules(string InShortName, ReadOnlyTargetRules Target) : base(Target)
    {
        // Ensure we're always using the latest build and include order versions, so that our
        // CI/CD server can pick up issues in new engine releases.
        DefaultBuildSettings = BuildSettingsVersion.Latest;
        IncludeOrderVersion = EngineIncludeOrderVersion.Latest;

        // Enforce all modules to have a short name during compilation, which helps reduce the
        // lengths of paths.
        ShortName = InShortName;

        // Ensure we target C++20, as we need support for co_await.
        CppStandard = CppStandardVersion.Cpp20;

        // When we're developing the plugin, we turn on REDPOINT_STRICT_BUILD to ensure that
        // compatibility with older engines.
#if UE_5_6_OR_LATER
        var isMac = OperatingSystem.IsMacOS();
#else
        var isMac = RuntimePlatform.IsMac;
#endif
        if (Environment.GetEnvironmentVariable("REDPOINT_STRICT_BUILD") == "1" ||
            (isMac && Unreal.EngineDirectory.FullName.StartsWith("/Volumes/Build/RemoteMac", StringComparison.OrdinalIgnoreCase)))
        {
            if (!bEmittedStrictModeMessage)
            {
                Console.WriteLine("Redpoint EOS: Enabling strict build to ensure version and IWYU compatibility...");
                bEmittedStrictModeMessage = true;
            }
            if (!Target.bForceUnityBuild)
            {
                bUseUnity = false;
            }
            else
            {
                bUseUnity = true;
                bMergeUnityFiles = true;
            }
            PCHUsage = PCHUsageMode.NoPCHs;

            // Make sure when we're targeting Development with REDPOINT_STRICT_BUILD turned on, we are
            // able to debug all parts of the Redpoint plugin while having the rest of the engine optimized.
            if (Target.Configuration == UnrealTargetConfiguration.Development)
            {
                OptimizeCode = CodeOptimization.InShippingBuildsOnly;
            }
        }
    }
}
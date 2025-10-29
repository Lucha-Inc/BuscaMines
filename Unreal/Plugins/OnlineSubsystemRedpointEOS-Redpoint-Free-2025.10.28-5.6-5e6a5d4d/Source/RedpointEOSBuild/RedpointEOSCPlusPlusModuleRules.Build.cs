// Copyright June Rhodes. All Rights Reserved.

using System;
using System.IO;
using System.Reflection;
using UnrealBuildTool;

/// <summary>
/// Provides a base implementation of ModuleRules that is used by all C++ modules
/// in the Redpoint EOS plugin.
/// </summary>
public abstract class RedpointEOSCPlusPlusModuleRules : RedpointEOSCommonModuleRules
{
    protected bool bIsNoEOS = false;

    protected RedpointEOSCPlusPlusModuleRules(string InShortName, ReadOnlyTargetRules Target) : base(InShortName, Target)
    {
        // This is the default, but be explicit about it.
        Type = ModuleType.CPlusPlus;

        // Compute the module name without any platform suffixes.
        var ModuleName = GetType().Name;
        var ModuleUnderscoreIndex = ModuleName.LastIndexOf("_");
        if (ModuleUnderscoreIndex != -1)
        {
            // Trim off any platform suffix e.g. 'ModuleName_Platform' -> 'ModuleName'.
            ModuleName = ModuleName[..ModuleUnderscoreIndex];
        }

        // If this isn't the RedpointEOSBuild module itself, depend on it.
        if (ModuleName != "RedpointEOSBuild")
        {
            PublicDependencyModuleNames.Add("RedpointEOSBuild");
        }

        // We currently turn a few things off for the 'Tests' modules.
        var bIsOlderModule = ModuleName.EndsWith("Tests");

        // Ensure all modules get the Unreal Engine version defines that we expect.
        // @note: For now this also adds include paths, until we migrate all files to the new
        // folder structure (as seen in RedpointEOSAPI).
        if (!GetType().Name.StartsWith("RedpointEOS") || bIsOlderModule)
        {
            PrivateIncludePaths.Add(Path.Combine(GetModuleDirectory("OnlineSubsystemRedpointEOS"), ".."));
            PublicIncludePaths.Add(Path.Combine(GetModuleDirectory("OnlineSubsystemRedpointEOS"), ".."));
        }

        // We no longer increase compiler warning levels here and instead leave it to our code guard macros. This avoids
        // warnings that are turned off in engine header files being elevated to errors.
#if UE_5_6_OR_LATER
        CppCompileWarningSettings.ModuleIncludePathWarningLevel = WarningLevel.Error;
        CppCompileWarningSettings.ModuleIncludePrivateWarningLevel = WarningLevel.Error;
        CppCompileWarningSettings.ModuleIncludeSubdirectoryWarningLevel = WarningLevel.Error;
#else
        ModuleIncludePathWarningLevel = WarningLevel.Error;
        ModuleIncludePrivateWarningLevel = WarningLevel.Error;
        ModuleIncludeSubdirectoryWarningLevel = WarningLevel.Error;
#endif

        // Detect if we're being built in "No EOS" mode.
        try
        {
            var pluginInfo = (PluginInfo)this.GetType()
                .GetProperty("Plugin", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);
            if (pluginInfo?.Name == "OnlineSubsystemBlueprintsOnly")
            {
                bIsNoEOS = true;
            }
        }
        catch
        {
        }

            bUsePrecompiled = true;


    }
}
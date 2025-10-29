// Copyright June Rhodes. All Rights Reserved.

using System;
using System.IO;
using System.Reflection;
using UnrealBuildTool;

/// <summary>
/// Provides a ModuleRules that provides headers to other modules without building an actual module.
/// </summary>
public abstract class RedpointEOSHeaderOnlyModuleRules : RedpointEOSExternalModuleRules
{
    protected RedpointEOSHeaderOnlyModuleRules(string InShortName, ReadOnlyTargetRules Target) : base(InShortName, Target)
    {
        // Compute the module name without any platform suffixes.
        var ModuleName = GetType().Name;
        var ModuleUnderscoreIndex = ModuleName.LastIndexOf("_");
        if (ModuleUnderscoreIndex != -1)
        {
            // Trim off any platform suffix e.g. 'ModuleName_Platform' -> 'ModuleName'.
            ModuleName = ModuleName[..ModuleUnderscoreIndex];
        }

        // This module is declared external so that we make our headers available, but the build system won't actually try to build a module for runtime.
        PublicIncludePaths.Add(Path.Combine(GetModuleDirectory(ModuleName), "Public"));
    }
}
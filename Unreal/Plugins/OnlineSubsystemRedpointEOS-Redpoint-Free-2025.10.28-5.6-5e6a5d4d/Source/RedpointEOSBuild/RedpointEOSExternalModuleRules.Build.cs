// Copyright June Rhodes. All Rights Reserved.

using System;
using System.Reflection;
using UnrealBuildTool;

/// <summary>
/// Provides a base implementation of ModuleRules that is used by all external modules
/// in the Redpoint EOS plugin.
/// </summary>
public abstract class RedpointEOSExternalModuleRules : RedpointEOSCommonModuleRules
{
    protected RedpointEOSExternalModuleRules(string InShortName, ReadOnlyTargetRules Target) : base(InShortName, Target)
    {
        // This module is an external module.
        Type = ModuleType.External;
    }
}
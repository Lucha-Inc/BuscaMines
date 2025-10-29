// Copyright June Rhodes. All Rights Reserved.

using EpicGames.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnrealBuildBase;
using UnrealBuildTool;

public interface IRedpointEOSSDKPlatform
{
    bool IsActive(ReadOnlyTargetRules Target);

    string GetPlatformSuffix();

    void ConfigureModule(string SdkBase, string ModuleBase, ModuleRules Module, ReadOnlyTargetRules Target);

    void ConfigureModule(string SdkBase, string ModuleBase, ModuleRules Module, ReadOnlyTargetRules Target, string EOSSDKFullVersion)
    {
        ConfigureModule(SdkBase, ModuleBase, Module, Target);
    }

    RedpointEOSSDKVersionConstraints GetSDKVersionConstraints();
}
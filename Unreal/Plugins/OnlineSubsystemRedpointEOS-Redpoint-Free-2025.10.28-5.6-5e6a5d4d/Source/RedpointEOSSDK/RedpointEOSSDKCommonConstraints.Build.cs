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

public static class RedpointEOSSDKCommonConstraints
{
    public static RedpointEOSSDKVersionConstraints GetDesktopSDKVersionConstraints()
    {
        // There are currently no constraints on desktop builds.
        return default;
    }
}
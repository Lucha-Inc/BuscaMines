// Copyright June Rhodes. All Rights Reserved.

using System;
using System.IO;
using UnrealBuildTool;

/// <summary>
/// Provides common build-related functions for the Redpoint EOS plugin.
/// </summary>
public class RedpointEOSBuild : RedpointEOSHeaderOnlyModuleRules
{
    public RedpointEOSBuild(ReadOnlyTargetRules Target) : base("REB", Target)
    {
        // Surface defines that allow us to conditionally include code
        // based on the engine version.
#if UE_5_7_OR_LATER
		PublicDefinitions.Add("REDPOINT_EOS_UE_5_7_OR_LATER=1");
#else
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_7_OR_LATER=0");
#endif
#if UE_5_6_OR_LATER
		PublicDefinitions.Add("REDPOINT_EOS_UE_5_6_OR_LATER=1");
#else
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_6_OR_LATER=0");
#endif
#if UE_5_5_OR_LATER
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_5_OR_LATER=1");
#else
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_5_OR_LATER=0");
#endif
#if UE_5_4_OR_LATER
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_4_OR_LATER=1");
#else
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_4_OR_LATER=0");
#endif
#if UE_5_3_OR_LATER
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_3_OR_LATER=1");
#else
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_3_OR_LATER=0");
#endif
#if UE_5_2_OR_LATER
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_2_OR_LATER=1");
#else
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_2_OR_LATER=0");
#endif
#if UE_5_1_OR_LATER
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_1_OR_LATER=1");
#else
        PublicDefinitions.Add("REDPOINT_EOS_UE_5_1_OR_LATER=0");
#endif

        // Defines the version of APIs for the purposes of console ports referring
        // to the main plugin code.
        PublicDefinitions.Add("REDPOINT_EOS_CONSOLE_INTERNAL_API_VERSION=10");

        // Add definitions that indicate the type of build we're doing.
        if (Target.Type == TargetType.Server)
        {
            PublicDefinitions.Add("REDPOINT_EOS_IS_DEDICATED_SERVER=1");
        }
        else
        {
            PublicDefinitions.Add("REDPOINT_EOS_IS_DEDICATED_SERVER=0");
        }
        if (Target.Type == TargetType.Server ||
            Target.Type == TargetType.Editor)
        {
            PublicDefinitions.Add("REDPOINT_EOS_CAN_ACT_AS_DEDICATED_SERVER=1");
        }
        else
        {
            PublicDefinitions.Add("REDPOINT_EOS_CAN_ACT_AS_DEDICATED_SERVER=0");
        }

        if (GetConfigBool(Target, "BUILDING_FREE_EDITION", false))
        {
            PublicDefinitions.Add("REDPOINT_EOS_IS_FREE_EDITION=1");
        }
        else
        {
            PublicDefinitions.Add("REDPOINT_EOS_IS_FREE_EDITION=0");
        }
    }

    public static bool GetConfigBool(ReadOnlyTargetRules Target, string Name, bool bDefaultValue)
    {
        return GetConfigBool(Target, Name, bDefaultValue, out _);
    }

    public static bool GetConfigBool(ReadOnlyTargetRules Target, string Name, bool bDefaultValue, out bool bExplicitlySet)
    {
        // Prepend the ONLINE_SUBSYSTEM_EOS_ prefix.
        Name = "ONLINE_SUBSYSTEM_EOS_" + Name;

        // Try to read from the environment variables first.
        var envValue = Environment.GetEnvironmentVariable(Name);
        if (envValue != null)
        {
            if (Name != "ONLINE_SUBSYSTEM_EOS_FORCE_SDK_USAGE_FROM_PLUGIN" &&
                Name != "ONLINE_SUBSYSTEM_EOS_BUILDING_FREE_EDITION")
            {
                Console.WriteLine("WARNING: The configuration value " + Name + " is being read from an environment variable. You should remove the environment variable from your system and migrate to using 'ProjectDefinitions.Add(\"" + Name + "=1\");' in your project targets instead.");
            }

            bExplicitlySet = true;
            return envValue == "true" || envValue == "1";
        }

        // Check to see if the setting is in the target's definitions.
        foreach (string definition in Target.ProjectDefinitions)
        {
            if (definition.StartsWith(Name + "="))
            {
                bExplicitlySet = true;
                return definition == Name + "=1";
            }
        }

        // Otherwise fallback to default.
        bExplicitlySet = false;
        return bDefaultValue;
    }

    public static string GetConfigString(ReadOnlyTargetRules Target, string Name, string DefaultValue)
    {
        // Prepend the ONLINE_SUBSYSTEM_EOS_ prefix.
        Name = "ONLINE_SUBSYSTEM_EOS_" + Name;

        // For permitted settings, try to read from the environment variables first.
        if (Name == "ONLINE_SUBSYSTEM_EOS_FORCE_SDK_VERSION" ||
            Name.StartsWith("ONLINE_SUBSYSTEM_EOS_FORCE_SDK_VERSION_") ||
            Name == "ONLINE_SUBSYSTEM_EOS_FORCE_SDK_MATCH")
        {
            var envValue = Environment.GetEnvironmentVariable(Name);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue;
            }
        }

        // Check to see if the setting is in the target's definitions.
        foreach (string definition in Target.ProjectDefinitions)
        {
            if (definition.StartsWith(Name + "="))
            {
                return definition.Split('=')[1];
            }
        }

        // Otherwise fallback to default.
        return DefaultValue;
    }
}
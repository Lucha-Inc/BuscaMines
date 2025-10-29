// Copyright June Rhodes. All Rights Reserved.

using EpicGames.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnrealBuildBase;
using UnrealBuildTool;

/// <summary>
/// Helper class that locates various SDKs, including the EOS SDK, based on RedpointEOSSDKInfo.json.
/// </summary>
public static class RedpointSDKLocator
{
    public class RedpointEOSSDKInfo
    {
        public Dictionary<string, string[]> SupportedVersions { get; set; } = new Dictionary<string, string[]>();

        public Dictionary<string, Dictionary<string, string[]>> SearchPaths { get; set; } = new Dictionary<string, Dictionary<string, string[]>>();

        public Dictionary<string, string> DisplayNames { get; set; } = new Dictionary<string, string>();
    }

    private static T DeserializeJson<T>(string json)
    {
        var SystemTextJsonAssembly = Assembly.Load("System.Text.Json") ?? throw new InvalidOperationException("Unable to load 'System.Text.Json' assembly.");
        var JsonSerializer = SystemTextJsonAssembly.GetType("System.Text.Json.JsonSerializer") ?? throw new InvalidOperationException("Unable to find 'System.Text.Json.JsonSerializer' type.");
        var DeserializeMethodGeneric = JsonSerializer
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(Method =>
            {
                var GenericArguments = Method.GetGenericArguments();
                var Parameters = Method.GetParameters();
                return (Method.Name == "Deserialize" &&
                    GenericArguments.Length == 1 &&
                    Parameters.Length == 2 &&
                    Parameters[0].ParameterType == typeof(string) &&
                    Parameters[1].ParameterType.Name == "JsonSerializerOptions");
            })
            .FirstOrDefault() ?? throw new InvalidOperationException("Unable to find 'Deserialize' method on JsonSerializer.");
        var DeserializeMethod = DeserializeMethodGeneric.MakeGenericMethod(new Type[] { typeof(T) });
        return (T)DeserializeMethod.Invoke(null, new object[] { json, null });
    }

    private static readonly Regex _searchPathVariableRegex = new("\\$\\{([a-zA-Z0-9:_]+)\\}");

    private static string[] ProcessSearchPaths(
        string[] SearchPaths,
        Dictionary<string, string> Variables,
        Dictionary<string, string> Folders)
    {
        var Results = new string[SearchPaths.Length];
        for (var i = 0; i < SearchPaths.Length; i++)
        {
            Results[i] = _searchPathVariableRegex
                .Replace(SearchPaths[i], (Match m) =>
                {
                    var Type = m.Groups[1].Value;
                    if (Type.StartsWith("V:") && Variables.TryGetValue(Type[2..], out var Variable))
                    {
                        return Variable;
                    }
                    else if (Type.StartsWith("F:") && Folders.TryGetValue(Type[2..], out var Folder))
                    {
                        return Folder;
                    }
                    else
                    {
                        return Environment.GetEnvironmentVariable(Type) ?? string.Empty;
                    }
                })
                .Replace('/', System.IO.Path.DirectorySeparatorChar)
                .Replace('\\', System.IO.Path.DirectorySeparatorChar);
        }
        return Results;
    }

    public static string LocateSdkDirectory(
        ModuleRules Module,
        ReadOnlyTargetRules Target,
        string SDKName,
        string PlatformSuffix,
        RedpointEOSSDKVersionConstraints Constraints,
        out string SDKFullVersionOut,
        ref List<string> EmitLogsOnFailure)
    {
        var bWasFound = false;
        var bEmitLogsOnFailure = false;

        if (EmitLogsOnFailure == null)
        {
            EmitLogsOnFailure = new List<string>();
            bEmitLogsOnFailure = true;
        }

        try
        {
            string ModuleBase = Module.ModuleDirectory;
            var RedpointEOSSDKInfoPath = Path.Combine(Module.PluginDirectory, "Source", "RedpointEOSSDK", "RedpointEOSSDKInfo.json");

            // Read the RedpointEOSSDKInfo.json file which contains our supported EOS SDK versions.
            Module.ExternalDependencies.Add(RedpointEOSSDKInfoPath);
            var RedpointEOSSDKInfoAll = DeserializeJson<RedpointEOSSDKInfo>(File.ReadAllText(RedpointEOSSDKInfoPath));
            if (!RedpointEOSSDKInfoAll.SupportedVersions.TryGetValue(SDKName, out var SupportedVersionsSDK))
            {
                EmitLogsOnFailure.Add($"ERROR: Missing 'SupportedVersions.{SDKName}' property in RedpointEOSSDKInfo.json file.");
                throw new Exception($"Missing 'SupportedVersions.{SDKName}' property in RedpointEOSSDKInfo.json file.");
            }
            if (!RedpointEOSSDKInfoAll.SearchPaths.TryGetValue(SDKName, out var SearchPathsEOSSDK))
            {
                EmitLogsOnFailure.Add($"ERROR: Missing 'SearchPaths.{SDKName}' property in RedpointEOSSDKInfo.json file.");
                throw new Exception($"Missing 'SearchPaths.{SDKName}' property in RedpointEOSSDKInfo.json file.");
            }
            if (!SearchPathsEOSSDK.TryGetValue("Windows", out var SearchPathsSDKWindows))
            {
                EmitLogsOnFailure.Add($"ERROR: Missing 'SearchPaths.{SDKName}.Windows' property in RedpointEOSSDKInfo.json file.");
                throw new Exception($"Missing 'SearchPaths.{SDKName}.Windows' property in RedpointEOSSDKInfo.json file.");
            }
            if (!SearchPathsEOSSDK.TryGetValue("Unix", out var SearchPathsSDKUnix))
            {
                EmitLogsOnFailure.Add($"ERROR: Missing 'SearchPaths.{SDKName}.Unix' property in RedpointEOSSDKInfo.json file.");
                throw new Exception($"Missing 'SearchPaths.{SDKName}.Unix' property in RedpointEOSSDKInfo.json file.");
            }
            if (!RedpointEOSSDKInfoAll.DisplayNames.TryGetValue(SDKName, out var SDKDisplayName))
            {
                EmitLogsOnFailure.Add($"ERROR: Missing 'DisplayNames.{SDKName}' property in RedpointEOSSDKInfo.json file.");
                throw new Exception($"Missing 'DisplayNames.{SDKName}' property in RedpointEOSSDKInfo.json file.");
            }

            var LogPrefix = $"Redpoint ({SDKDisplayName})";

            // Detect if this is the Free Edition, which will influence how we find the SDK.
            var bIsFreeEdition = true;


            // If we're the Marketplace version, allow the developer to use any SDK.
            IEnumerable<string> SearchVersionsSDK;
            if (!bIsFreeEdition)
            {
                // Allow the environment to force a specific SDK version.
                var ForcedSDKVersion = RedpointEOSBuild.GetConfigString(Target, "FORCE_SDK_VERSION" + PlatformSuffix.Replace("-", "_"), "");
                if (string.IsNullOrWhiteSpace(ForcedSDKVersion))
                {
                    ForcedSDKVersion = RedpointEOSBuild.GetConfigString(Target, "FORCE_SDK_VERSION", "");
                }

                // Filter the supported versions based on FORCE_SDK_MATCH.
                SearchVersionsSDK = SupportedVersionsSDK;
                var SDKMatchMode = RedpointEOSBuild.GetConfigString(Target, "FORCE_SDK_MATCH", "");
                if (string.IsNullOrWhiteSpace(ForcedSDKVersion) && !string.IsNullOrWhiteSpace(SDKMatchMode))
                {
                    if (SDKMatchMode == "Latest")
                    {
                        SearchVersionsSDK = new List<string>
                        {
                            SupportedVersionsSDK.First(),
                        };
                    }
                    else if (SDKMatchMode == "Oldest")
                    {
                        SearchVersionsSDK = new List<string>
                        {
                            SupportedVersionsSDK.Last(),
                        };
                    }
                    else
                    {
                        EmitLogsOnFailure.Add("ERROR: Unsupported FORCE_SDK_VERSION option.");
                        throw new Exception("Unsupported FORCE_SDK_VERSION option.");
                    }
                }
            }
            else
            {
                // Otherwise, the Free Edition always uses the latest SDK version.
                SearchVersionsSDK = new List<string>
                {
                    SupportedVersionsSDK.First(),
                };
            }

            // Search for the SDK.
            var bIsMaximumTolerableConstrained = Constraints.MaximumTolerableSDKVersion != null;
            foreach (var SDKFullVersion in SearchVersionsSDK)
            {
                var bIsThisVersionConstrained = false;
                if (Constraints.MaximumTolerableSDKVersion != null && bIsMaximumTolerableConstrained)
                {
                    bIsThisVersionConstrained = true;

                    if (Constraints.MaximumTolerableSDKVersion == SDKFullVersion)
                    {
                        bIsMaximumTolerableConstrained = false;
                    }
                }
                if (Constraints.SDKVersionDenyList != null && Constraints.SDKVersionDenyList.Contains(SDKFullVersion))
                {
                    bIsThisVersionConstrained = true;
                }

                string[] CandidateDirectories;
                if (RedpointEOSBuild.GetConfigBool(Target, "FORCE_SDK_USAGE_FROM_PLUGIN", false))
                {
                    if (SDKName == "EOSSDK-Unversioned")
                    {
                        CandidateDirectories = new string[]
                        {
                            // Where the "SDK" folder extracted from the ZIP has been renamed to "ThirdParty" and placed next to "OnlineSubsystemRedpointEOS".
                            Path.Combine(Path.GetFullPath(ModuleBase), "..", "ThirdParty" + PlatformSuffix),
                        };
                    }
                    else
                    {
                        // We don't search any candidate directories except for EOSSDK-Unversioned when force SDK usage from plugin is turned on.
                        CandidateDirectories = Array.Empty<string>();
                    }
                }
#if UNSUPPORTED_DUE_TO_MISSING_FILES_IN_INSTALLED_ENGINES
                else if (RedpointEOSBuild.GetConfigBool(Target, "FORCE_SDK_USAGE_FROM_ENGINE", false))
                {
                    if (SDKName == "EOSSDK-Unversioned")
                    {
                        CandidateDirectories = new string[]
                        {
                            // Only use the SDK from the engine.
                            Path.Combine(Module.EngineDirectory, "Source", "ThirdParty", "EOSSDK", "SDK"),
                            Path.Combine(Module.EngineDirectory, "Platforms", PlatformSuffix.TrimStart('-'), "Source", "ThirdParty", "EOSSDK", "SDK"),
                        };
                    }
                    else
                    {
                        // We don't search any candidate directories except for EOSSDK-Unversioned when force SDK usage from engine is turned on.
                        CandidateDirectories = Array.Empty<string>();
                    }
                }
#endif
                else
                {
                    var Variables = new Dictionary<string, string>
                    {
                        { "EngineDirectory", Path.GetFullPath(Module.EngineDirectory).TrimEnd('/').TrimEnd('\\') },
                        {
                            "ProjectDirectory",
                            Target.ProjectFile != null
                                ? Target.ProjectFile.Directory.FullName.TrimEnd('/').TrimEnd('\\')
                                : Module.EngineDirectory.TrimEnd('/').TrimEnd('\\')
                        },
                        { "ModuleBase", Path.GetFullPath(ModuleBase) },
                        { "ModuleDirectory", Path.GetFullPath(ModuleBase) },
                        { "PluginDirectory", Path.GetFullPath(Module.PluginDirectory) },
                        { "PlatformSuffix", PlatformSuffix },
                        { "Platform", PlatformSuffix.TrimStart('-') },
                        { "FullVersion", SDKFullVersion },
                    };
                    var Folders = new Dictionary<string, string>
                    {
                        { "CommonApplicationData", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) },
                        { "UserProfile", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) },
                    };
                    if (Path.DirectorySeparatorChar == '\\' && SearchPathsSDKWindows != null)
                    {
                        CandidateDirectories = ProcessSearchPaths(
                            SearchPathsSDKWindows,
                            Variables,
                            Folders);
                    }
                    else if (SearchPathsSDKUnix != null)
                    {
                        CandidateDirectories = ProcessSearchPaths(
                            SearchPathsSDKUnix,
                            Variables,
                            Folders);
                    }
                    else
                    {
                        throw new InvalidOperationException("Should not be possible to have no candidate directories to search after earlier checks!");
                    }
                }

                foreach (var Dir in CandidateDirectories)
                {
                    if (!bIsThisVersionConstrained)
                    {
                        try
                        {
                            EmitLogsOnFailure.Add($"{LogPrefix}: Searching for {SDKName} in path: " + new DirectoryInfo(Dir).FullName);
                        }
                        catch
                        {
                            EmitLogsOnFailure.Add($"{LogPrefix}: Searching for {SDKName} in path: " + Dir);
                        }
                    }
                    if (Directory.Exists(Dir))
                    {
                        if (bIsThisVersionConstrained)
                        {
                            EmitLogsOnFailure.Add($"WARNING: Found {SDKDisplayName}, but platform constraints meant that it couldn't be chosen as the SDK for the build. You probably need to install a different version of the SDK. The {SDKDisplayName} was found at: " + Dir);
                        }
                        else if (PlatformSuffix == "-IOS"
                            && !Directory.Exists(Path.Combine(Dir, "Bin", "IOS", "EOSSDK.framework", "Headers"))
                            && SDKName == "EOSSDK")
                        {
                            // @note: ThirdParty-IOS may be missing files when copied for Remote Mac builds. Refuse to use a ThirdParty-IOS folder
                            // if it's missing the headers necessary for the build to work.
                            EmitLogsOnFailure.Add($"WARNING: Ignoring the {SDKDisplayName} for iOS that was found because it is missing some required files. This can happen when using 'Remote Mac' builds from Windows, where the engine does not correctly copy all required files to the remote macOS machine. If the build is failing and you're seeing this warning, try installing the {SDKDisplayName} underneath the 'Downloads' folder on the remote macOS machine, as per the documentation (https://docs.redpoint.games/eos-online-subsystem/docs/#installing-the-eos-sdk). The {SDKDisplayName} directory being ignored was: " + Dir);
                        }
                        else
                        {
                            EmitLogsOnFailure.Add($"{LogPrefix}: Located {SDKDisplayName} at: " + Dir);
                            SDKFullVersionOut = SDKFullVersion;
                            bWasFound = true;
                            return Dir;
                        }
                    }
                }
            }

            SDKFullVersionOut = "";
            return null;
        }
        finally
        {
            if (!bWasFound && bEmitLogsOnFailure)
            {
                foreach (var Log in EmitLogsOnFailure)
                {
                    Console.WriteLine(Log);
                }
            }
        }
    }
}
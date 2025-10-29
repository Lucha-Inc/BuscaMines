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

public class RedpointEOSSDK : RedpointEOSExternalModuleRules
{
    public RedpointEOSSDK(ReadOnlyTargetRules Target) : base("RES", Target)
    {
        // Locate the platform implementation.
        IRedpointEOSSDKPlatform SelectedPlatform = null;
        var EmitLogsOnFailure = new List<string>();
        foreach (var Assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] Types;
            try
            {
                Types = Assembly.GetTypes();
            }
            catch
            {
                continue;
            }
            foreach (var PlatformImplementationType in Types.Where(x => typeof(IRedpointEOSSDKPlatform).IsAssignableFrom(x) && x.IsClass))
            {
                EmitLogsOnFailure.Add("EOSSDK: Considering platform implementation " + PlatformImplementationType.Name + "...");
                var PlatformCandidate = (IRedpointEOSSDKPlatform)Activator.CreateInstance(PlatformImplementationType);
                if (PlatformCandidate.IsActive(Target))
                {
                    EmitLogsOnFailure.Add("EOSSDK: Selected platform implementation " + PlatformImplementationType.Name + ".");
                    SelectedPlatform = PlatformCandidate;
                    break;
                }
            }
        }
        if (SelectedPlatform == null)
        {
            EmitLogsOnFailure.Add("ERROR: The EOS SDK is not supported on this platform. Either update .Build.cs or remove the platform from the WhitelistPlatforms setting in the .uplugin file.");
            foreach (var Log in EmitLogsOnFailure)
            {
                Console.WriteLine(Log);
            }
            throw new Exception("EOS SDK not available for this platform");
        }

        // Determine the platform suffix.
        var PlatformSuffix = SelectedPlatform.GetPlatformSuffix();

        // Locate the SDK.
        var SdkErrorLogs = new List<string>();
        var SdkBase = RedpointSDKLocator.LocateSdkDirectory(
            this,
            Target,
            "EOSSDK",
            PlatformSuffix,
            SelectedPlatform.GetSDKVersionConstraints(),
            out var EOSSDKFullVersion,
            ref SdkErrorLogs);
        if (SdkBase == null)
        {
            PublicDefinitions.Add("REDPOINT_EOS_SDK_IS_UNVERSIONED_AT_BUILD_TIME=1");
            SdkBase = RedpointSDKLocator.LocateSdkDirectory(
                this,
                Target,
                "EOSSDK-Unversioned",
                PlatformSuffix,
                SelectedPlatform.GetSDKVersionConstraints(),
                out EOSSDKFullVersion,
                ref SdkErrorLogs);
        }
        else
        {
            PublicDefinitions.Add("REDPOINT_EOS_SDK_IS_UNVERSIONED_AT_BUILD_TIME=0");
        }
        if (SdkBase == null)
        {
            foreach (var ErrorLog in SdkErrorLogs)
            {
                Console.WriteLine(ErrorLog);
            }
            Console.WriteLine("ERROR: Could not locate the EOS SDK. Please download it from the Epic Games developer portal and extract it in one of the supported locations.");
            throw new Exception("EOS SDK not found");
        }

        // Wire everything up.
        SelectedPlatform.ConfigureModule(SdkBase, ModuleDirectory, this, Target, EOSSDKFullVersion);

        Console.WriteLine($"Redpoint (EOS SDK): Using the EOS SDK at this path: {SdkBase}");

        PublicDefinitions.Add("EOS_SDK_LINK_VERSION=\"" + EOSSDKFullVersion + "\"");

        if (EOSSDKFullVersion.Contains("-hf-"))
        {
            // If this version is a hotfix version, including the hotfix version information.
            var HotfixVersion = EOSSDKFullVersion[(EOSSDKFullVersion.IndexOf("-hf-") + "-hf-".Length)..];
            if (!int.TryParse(HotfixVersion, out var HotfixVersionInt))
            {
                throw new Exception("Unable to parse hotfix version number '" + HotfixVersion + "' from EOS SDK folder name!");
            }
            PublicDefinitions.Add("EOS_HOTFIX_VERSION=" + HotfixVersionInt + "");
        }

        // Use the new platform-specific build infrastructure.
        if (!Target.IsInPlatformGroup(UnrealPlatformGroup.Windows) &&
            !Target.IsInPlatformGroup(UnrealPlatformGroup.Unix) &&
            !Target.IsInPlatformGroup(UnrealPlatformGroup.Apple) &&
            !Target.IsInPlatformGroup(UnrealPlatformGroup.Android))
        {
            var bHasEOSBuildPlatformNameOverride = false;
            foreach (string Definition in PublicDefinitions)
            {
                if (Definition.StartsWith("EOS_BUILD_PLATFORM_NAME="))
                {
                    bHasEOSBuildPlatformNameOverride = true;
                    break;
                }
            }
            if (!bHasEOSBuildPlatformNameOverride)
            {
                PublicDefinitions.Add("EOS_BUILD_PLATFORM_NAME=" + Target.Platform.ToString() + "");
            }
        }
    }

    public static void EnsureDirectoryExists(string[] Paths)
    {
        foreach (var RelPath in Paths)
        {
            var FullPath = Path.GetFullPath(RelPath);
            if (string.IsNullOrWhiteSpace(FullPath))
            {
                Console.WriteLine("ERROR: Unable to locate a directory that was expected to exist in the EOS SDK. Download and re-extract the SDK to ensure that it has all the required directories. The file that was missing was: " + RelPath);
                throw new Exception("EOS SDK is missing one or more required files");
            }
            if (!Directory.Exists(FullPath))
            {
                Console.WriteLine("ERROR: Unable to locate a directory that was expected to exist in the EOS SDK. Download and re-extract the SDK to ensure that it has all the required directories. The file that was missing was: " + FullPath);
                throw new Exception("EOS SDK is missing one or more required files");
            }
        }
    }

    public static void EnsureFilesExist(string[] Paths)
    {
        foreach (var RelPath in Paths)
        {
            var FullPath = Path.GetFullPath(RelPath);
            if (string.IsNullOrWhiteSpace(FullPath))
            {
                Console.WriteLine("ERROR: Unable to locate a file that was expected to exist in the EOS SDK. Download and re-extract the SDK to ensure that it has all the required files. The file that was missing was: " + RelPath);
                throw new Exception("EOS SDK is missing one or more required files");
            }
            if (!File.Exists(FullPath))
            {
                Console.WriteLine("ERROR: Unable to locate a file that was expected to exist in the EOS SDK. Download and re-extract the SDK to ensure that it has all the required files. The file that was missing was: " + FullPath);
                throw new Exception("EOS SDK is missing one or more required files");
            }
        }
    }

    public static void StripQuarantineBit(string Path)
    {
#if UE_5_6_OR_LATER
        if (OperatingSystem.IsMacOS())
#else
        if (RuntimePlatform.IsMac)
#endif
        {
            try
            {
                var xattrProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "/usr/bin/xattr",
                    ArgumentList = { "-d", "com.apple.quarantine", Path },
                    CreateNoWindow = true,
                    UseShellExecute = false,
                });
                xattrProcess?.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"warning: Unable to remove quarantine bit from file '{Path}': {ex}");
            }
        }
    }

    /// <summary>
    /// When we want to run unit tests on device, they won't be able to authenticate with EAS unless we copy across the credentials from the current machine onto the device we are building for. This function is used by devices to find the file to copy.
    /// 
    /// EAS credentials are never available in shipping builds.
    /// </summary>
    /// <returns></returns>
    public static string LocateEASAutomatedTestingCredentialsPath(ReadOnlyTargetRules Target)
    {
        if (Target.Configuration == UnrealTargetConfiguration.Shipping)
        {
            return null;
        }

        var searchablePaths = new List<string>();
        if (Environment.GetEnvironmentVariable("EAS_AUTOMATED_TESTING_CREDENTIAL_PATH") != null)
        {
            searchablePaths.Add(Environment.GetEnvironmentVariable("EAS_AUTOMATED_TESTING_CREDENTIAL_PATH"));
        }
        searchablePaths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Projects", "eas-automated-testing-credentials", "Credentials.json"));
        foreach (var path in searchablePaths)
        {
            if (File.Exists(path))
            {
                Console.WriteLine("EOSSDK (Unit Tests): Located automated testing credentials at path: " + path + ". They will be packaged with the game.");
                return path;
            }
        }
        return null;
    }
}
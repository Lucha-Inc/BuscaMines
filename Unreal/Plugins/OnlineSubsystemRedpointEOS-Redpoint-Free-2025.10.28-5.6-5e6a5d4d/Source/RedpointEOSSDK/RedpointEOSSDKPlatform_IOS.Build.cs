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

/// <summary>
/// iOS platform support for EOS.
/// </summary>
public class RedpointEOSSDKPlatform_IOS : IRedpointEOSSDKPlatform
{
    public bool IsActive(ReadOnlyTargetRules Target)
    {
        return Target.Platform == UnrealTargetPlatform.IOS;
    }

    public string GetPlatformSuffix()
    {
        return "-IOS";
    }

    public void ConfigureModule(string SdkBase, string ModuleBase, ModuleRules Module, ReadOnlyTargetRules Target)
    {
        Module.PublicIncludePaths.Add(Path.Combine(SdkBase, "Bin", "IOS", "EOSSDK.framework", "Headers"));
        Module.ExternalDependencies.Add(Path.Combine(SdkBase, "Bin", "IOS", "EOSSDK.framework", "Headers", "eos_version.h"));
        Module.AdditionalPropertiesForReceipt.Add("IOSPlugin", Path.Combine(ModuleBase, "RedpointEOSSDK_IOS_UPL.xml"));

        // If the user has EAS credentials installed for unit tests, copy across the JSON file so that the test can
        // execute on device.
        var easCredentialsPath = RedpointEOSSDK.LocateEASAutomatedTestingCredentialsPath(Target);
        if (easCredentialsPath != null)
        {
            Module.RuntimeDependencies.Add("$(TargetOutputDir)/Credentials.json", easCredentialsPath);
        }

        // Ensure we're targeting iOS 14 or later. Earlier runtimes would use SafariServices and not use PresentationContext code, but these runtimes are no longer supported by Unreal Engine 5.1 and later, so we can safely drop support for those code paths.
        if (Target.IOSPlatform.RuntimeVersion < 14.0f)
        {
            Console.WriteLine("error: EOSSDK (IOS): This project is targeting an iOS runtime version earlier than 14.0, which is not supported by this version of Unreal Engine or this version of the Redpoint EOS plugin. The current runtime version is: " + Target.IOSPlatform.RuntimeVersion);
            throw new Exception("EOSSDK (IOS): This project is targeting an iOS runtime version earlier than 14.0, which is not supported by this version of Unreal Engine or this version of the Redpoint EOS plugin. The current runtime version is: " + Target.IOSPlatform.RuntimeVersion);
        }

        // Add a dependency on AuthenticationServices, which is required for iOS authentication.
        Module.PublicFrameworks.Add("AuthenticationServices");

#if UE_5_3_OR_LATER
		// Embed and sign the EOS SDK framework.
		Module.PublicAdditionalFrameworks.Add(new ModuleRules.Framework(
			"EOSSDK",
			Path.Combine(SdkBase, "Bin", "IOS"),
			ModuleRules.Framework.FrameworkMode.LinkAndCopy,
			string.Empty));

		RedpointEOSSDK.EnsureDirectoryExists(new[] {
			Path.Combine(SdkBase, "Bin", "IOS", "EOSSDK.framework", "Headers"),
		});
		RedpointEOSSDK.EnsureFilesExist(new[] {
			Path.Combine(SdkBase, "Bin", "IOS", "EOSSDK.framework", "Headers", "eos_version.h"),
		});
#else
        // We have to generate a ZIP file, because that is the only way Unreal Engine supports adding
        // external frameworks on iOS.
        var FrameworkDirectory = Path.Combine(SdkBase, "Bin", "IOS", "EOSSDK.framework");
        var FrameworkPath = Path.Combine(SdkBase, "Bin", "IOS", "EOSSDK.embeddedframework.zip");
        if (Environment.GetEnvironmentVariable("GITLAB_CI") == "true" &&
            Environment.GetEnvironmentVariable("CI_PROJECT_NAMESPACE") == "redpointgames")
        {
            // Allow us to support builds in parallel on the same machine.
            Directory.CreateDirectory(Path.Combine(SdkBase, "Bin", "IOS", "CI-" + Environment.GetEnvironmentVariable("CI_CONCURRENT_PROJECT_ID")));
            FrameworkPath = Path.Combine(SdkBase, "Bin", "IOS", "CI-" + Environment.GetEnvironmentVariable("CI_CONCURRENT_PROJECT_ID"), "EOSSDK.embeddedframework.zip");
            Console.WriteLine("EOSSDK (IOS): Setting FrameworkPath to " + FrameworkPath + " because this build is running on GitLab CI/CD.");
        }
        if (File.Exists(FrameworkPath))
        {
            if (Environment.GetEnvironmentVariable("GITLAB_CI") == "true" &&
                Environment.GetEnvironmentVariable("CI_PROJECT_NAMESPACE") == "redpointgames")
            {
                // Ensure we always attempt to run the zip command below when building on GitLab CI/CD, to ensure that the command
                // continues to work with newer version of macOS and Xcode, and isn't reliant on a cached version of the file.
                Console.WriteLine("EOSSDK (IOS): Deleting cached EOSSDK.embeddedframework.zip to ensure correctness because this build is running on GitLab CI/CD.");
                File.Delete(FrameworkPath);
            }
        }
        var tempPath = Path.GetTempPath();
        if (Environment.GetEnvironmentVariable("GITLAB_CI") == "true" &&
            Environment.GetEnvironmentVariable("CI_PROJECT_NAMESPACE") == "redpointgames")
        {
            // Allow us to support builds in parallel on the same machine.
            tempPath = Path.Combine(Path.GetTempPath(), "CI-" + Environment.GetEnvironmentVariable("CI_CONCURRENT_PROJECT_ID"));
            Directory.CreateDirectory(tempPath);
        }
        if (!File.Exists(FrameworkPath) && Directory.Exists(FrameworkDirectory))
        {
            var target = Path.Combine(tempPath, "EOSSDK.embeddedframework");
            Console.WriteLine("EOSSDK (IOS): Generating EOSSDK.embeddedframework.zip for Unreal Engine on iOS...");
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }
            Directory.CreateDirectory(target);
            Directory.CreateDirectory(Path.Combine(target, "EOSSDK.embeddedframework"));
            DirectoryCopy(
                FrameworkDirectory,
                Path.Combine(target, "EOSSDK.embeddedframework", "EOSSDK.framework"),
                true);
            Console.WriteLine("EOSSDK (IOS): Using ZipFile.CreateFromDirectory to generate ZIP...");
            var assembly = Assembly.Load("System.IO.Compression.ZipFile");
            var type = assembly.GetType("System.IO.Compression.ZipFile");
            var method = type.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x => x.Name == "CreateFromDirectory" && x.GetParameters().Length == 4);
            var compressionAssembly = Assembly.Load("System.IO.Compression");
            var fastestValue = Enum.GetValues(compressionAssembly.GetType("System.IO.Compression.CompressionLevel")).GetValue(1);
            method.Invoke(null, new object[] {
                target,
                FrameworkPath,
                fastestValue,
                false
            });
            Console.WriteLine("EOSSDK (IOS): Generated EOSSDK.embeddedframework.zip for iOS to: " + FrameworkPath);
        }
        else if (File.Exists(FrameworkPath))
        {
            Console.WriteLine("EOSSDK (IOS): Using cached EOSSDK.embeddedframework.zip for Unreal Engine on iOS.");
        }

        // Add the EOS SDK as an embedded framework.
        Module.PublicAdditionalFrameworks.Add(new ModuleRules.Framework(
            "EOSSDK",
            FrameworkPath,
            ""
        ));

        // Generate a Frameworks folder and tell the build to copy it into the
        // app bundle with AdditionalBundleResources. UE4's build process does not
        // currently support embedding dynamic frameworks, so we have to do it
        // manually.
        var frameworksDir = Path.Combine(tempPath, "EOSSDK-Frameworks", "Frameworks");
        Directory.CreateDirectory(Path.Combine(frameworksDir, "EOSSDK.framework"));
        if (!File.Exists(Path.Combine(frameworksDir, "EOSSDK.framework", "EOSSDK")))
        {
            File.Copy(
                Path.Combine(FrameworkDirectory, "EOSSDK"),
                Path.Combine(frameworksDir, "EOSSDK.framework", "EOSSDK")
            );
        }
        if (!File.Exists(Path.Combine(frameworksDir, "EOSSDK.framework", "Info.plist")))
        {
            File.Copy(
                Path.Combine(FrameworkDirectory, "Info.plist"),
                Path.Combine(frameworksDir, "EOSSDK.framework", "Info.plist")
            );
        }
        Module.AdditionalBundleResources.Add(new ModuleRules.BundleResource(frameworksDir));

        RedpointEOSSDK.EnsureDirectoryExists(new[] {
            Path.Combine(FrameworkDirectory, "Headers"),
            frameworksDir,
        });
        RedpointEOSSDK.EnsureFilesExist(new[] {
            Path.Combine(FrameworkDirectory, "Headers", "eos_version.h"),
            FrameworkPath,
        });
#endif
    }

    private static void DirectoryCopy(string sourceDirName, string destDirName,
                                      bool copySubDirs)
    {
        var dir = new DirectoryInfo(sourceDirName);
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }
        var dirs = dir.GetDirectories();
        if (!Directory.Exists(destDirName))
        {
            _ = Directory.CreateDirectory(destDirName);
        }
        var files = dir.GetFiles();
        foreach (var file in files)
        {
            var temppath = Path.Combine(destDirName, file.Name);
            _ = file.CopyTo(temppath, false);
        }
        if (copySubDirs)
        {
            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
    }

    public RedpointEOSSDKVersionConstraints GetSDKVersionConstraints()
    {
        return default;
    }
}
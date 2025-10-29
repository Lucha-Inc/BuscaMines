// Copyright June Rhodes. All Rights Reserved.

using EpicGames.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnrealBuildTool;

/// <summary>
/// Android platform support for EOS.
/// </summary>
public class RedpointEOSSDKPlatform_Android : IRedpointEOSSDKPlatform
{
    public bool IsActive(ReadOnlyTargetRules Target)
    {
        return Target.Platform == UnrealTargetPlatform.Android;
    }

    public string GetPlatformSuffix()
    {
        return "-Android";
    }

    public void ConfigureModule(string SdkBase, string ModuleBase, ModuleRules Module, ReadOnlyTargetRules Target)
    {
        throw new NotSupportedException("This is a deprecated method; use the version that passes the SDK version instead.");
    }

    public void ConfigureModule(string SdkBase, string ModuleBase, ModuleRules Module, ReadOnlyTargetRules Target, string EOSSDKFullVersion)
    {
        var IsSplitSDK = Directory.Exists(Path.Combine(SdkBase, "Bin", "Android", "static-stdc++"));
        if (!IsSplitSDK)
        {
            Console.WriteLine("error: This EOS SDK version is too old to build for Android with any Unreal Engine version. Please upgrade to EOS SDK 1.15.4 or later.");
            throw new InvalidOperationException("This EOS SDK version is too old to build for Android any Unreal Engine version. Please upgrade to EOS SDK 1.15.4 or later.");
        }

        // Locate the AAR file.
        var AARDirectoryPath = Path.Combine(SdkBase, "Bin", "Android", "static-stdc++", "aar");
        var AARFilePath = Path.Combine(AARDirectoryPath, "eos-sdk.aar");
        if (!File.Exists(AARFilePath))
        {
            AARFilePath = Path.Combine(AARDirectoryPath, "eossdk-StaticSTDC-release.aar");
        }
        if (!File.Exists(AARFilePath))
        {
            Console.WriteLine("error: Expected 'eos-sdk.aar' or 'eossdk-StaticSTDC-release.aar' to exist in the Android EOS SDK underneath: " + AARDirectoryPath);
            throw new InvalidOperationException("Expected 'eos-sdk.aar' or 'eossdk-StaticSTDC-release.aar' to exist in the Android EOS SDK: " + AARDirectoryPath);
        }

        // If we don't have the dynamic libraries, extract them out of the AAR now.
        var Architectures = new string[] { "armeabi-v7a", "arm64-v8a", "x86", "x86_64" };
        var ExpectedLibraryFiles = new List<string>();
        var bAnyArchitectureMissing = false;
        foreach (var Architecture in Architectures)
        {
            var LibPath = Path.Combine(SdkBase, "Bin", "Android", "static-stdc++", "libs", Architecture, "libEOSSDK.so");
            if (!File.Exists(LibPath))
            {
                bAnyArchitectureMissing = true;
            }
            ExpectedLibraryFiles.Add(LibPath);
        }
        if (bAnyArchitectureMissing)
        {
            using var ArchiveStream = new FileStream(AARFilePath, FileMode.Open);
            var AssemblyRef = Assembly.Load("System.IO.Compression");
            var AssemblyZipRef = Assembly.Load("System.IO.Compression.ZipFile");
            var ExtensionsType = AssemblyZipRef.GetType("System.IO.Compression.ZipFileExtensions");
            var ExtensionsExtractToFileMethod = ExtensionsType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(x => x.Name == "ExtractToFile" && x.GetParameters().Length == 3);
            var ZipArchiveType = AssemblyRef.GetType("System.IO.Compression.ZipArchive");
            var ZipArchive = ZipArchiveType.GetConstructors()
                .First(x => x.GetParameters().Length == 1)
                .Invoke(new object[] { ArchiveStream });
            try
            {
                var ZipEntriesProperty = ZipArchiveType.GetProperty("Entries");
                var ZipEntries = (ICollection)ZipEntriesProperty.GetValue(ZipArchive);
                foreach (var ZipEntry in ZipEntries)
                {
                    var ZipEntryType = ZipEntry.GetType();
                    var ZipEntryFullNameProperty = ZipEntryType.GetProperty("FullName");
                    var ZipEntryFullName = (string)ZipEntryFullNameProperty.GetValue(ZipEntry);
                    foreach (var Architecture in Architectures)
                    {
                        var LibPath = Path.Combine(SdkBase, "Bin", "Android", "static-stdc++", "libs", Architecture, "libEOSSDK.so");
                        var RelativePath = Path.Combine("jni", Architecture, "libEOSSDK.so").Replace('\\', '/');
                        if (string.Equals(ZipEntryFullName, RelativePath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            Console.WriteLine("EOS SDK (Android): Extracting '" + ZipEntryFullName + "' to '" + LibPath + "'...");
                            _ = Directory.CreateDirectory(Path.GetDirectoryName(LibPath));
                            _ = ExtensionsExtractToFileMethod.Invoke(null, new object[] { ZipEntry, LibPath, true });
                        }
                    }
                }

            }
            finally
            {
                ((IDisposable)ZipArchive).Dispose();
            }
        }
        RedpointEOSSDK.EnsureFilesExist(ExpectedLibraryFiles.ToArray());

        // Add all of the expected library files.
        foreach (var LibraryFile in ExpectedLibraryFiles)
        {
            Module.PublicAdditionalLibraries.Add(LibraryFile);
        }

        // Add the include directory.
        string AndroidIncludeBase;
        if (Directory.Exists(Path.Combine(SdkBase, "Include")))
        {
            AndroidIncludeBase = Path.Combine(SdkBase, "Include");
        }
        else
        {
            AndroidIncludeBase = Path.Combine(SdkBase, "Bin", "Android", "include");
        }
        Module.PublicIncludePaths.Add(AndroidIncludeBase);
        Module.ExternalDependencies.Add(Path.Combine(AndroidIncludeBase, "eos_version.h"));

        // If the user has EAS credentials installed for unit tests, copy across the JSON file so that the test can
        // execute on device.
        var EASCredentialsPath = RedpointEOSSDK.LocateEASAutomatedTestingCredentialsPath(Target);
        if (EASCredentialsPath != null)
        {
            Module.RuntimeDependencies.Add("$(TargetOutputDir)/Credentials.json", EASCredentialsPath);
        }

        // Copy the EOS SDK .aar file as a runtime dependency into the target output directory. This isn't actually used
        // or bundled at runtime, but it ensures the .aar file gets copied into a known path *and* ensures it will get
        // tagged by BuildGraph.
        //
        // Later on our RedpointEOSSDK_Android_UPL.xml file will instruct Gradle to depend on the .aar located
        // in Gradle's OUTPUT_PATH property, which maps to the target output directory that BuildGraph will have staged
        // artifacts from previous build steps to.
        Module.RuntimeDependencies.Add("$(TargetOutputDir)/eossdk.aar", AARFilePath);

        // Add the our UPL file that contains our common settings regardless of the EOS SDK version, including the logic
        // that adds a dependency on eossdk.aar.
        var UPLFiles = new List<string>
        {
            "RedpointEOSSDK_Android_UPL.xml",
        };

        // Add our version-dependent UPL file, which changes the runtime Java dependencies of the Android application depending
        // on what the EOS SDK requires.
        switch (EOSSDKFullVersion)
        {
            case "23143668-v1.15.4":
            case "24099393-v1.15.5":
            case "27024038-v1.16":
            case "27379709-v1.16.1":
            case "30020700-v1.16.1-new":
                UPLFiles.Add("RedpointEOSSDK_Android_NativeDeps_1.15.4_UPL.xml");
                break;
            case "32273396-v1.16.2":
            case "32303053-v1.16.3":
            case "35276460-v1.16.3":
            case "36651368-v1.16.4":
                UPLFiles.Add("RedpointEOSSDK_Android_NativeDeps_1.16.2_UPL.xml");
                break;
            // The latest support EOS SDK should always fall under the default branch so that this code behaves correctly
            // for newer EOS SDK versions automatically.
            default:
                UPLFiles.Add("RedpointEOSSDK_Android_NativeDeps_1.17.0_UPL.xml");
                break;
        }

        // When building under BuildGraph across multiple machines, and the plugin is not installed in the engine, and each
        // machine uses a working directory with a different root path, the "AdditionalPropertiesForReceipt" would contain
        // an absolute path to a UPL, which would not exist on the node that runs the "Pak and Stage" step (only on the
        // "Compile" step).
        //
        // Unfortunately, "AdditionalPropertiesForReceipt" is hard-coded to be relative to the engine path. There's no reasonable
        // way to patch the engine (i.e. like the EAC hooks), since the patches would have to be quite significant to pass the
        // relevant context all the way to the UPL evaluation.
        //
        // To workaround this issue when building under UET/BuildGraph, we emit additional properties that get embedded in
        // the .target file, and then UET's BuildPlugin.xml BuildGraph script runs an additional step before "BuildCookRun"
        // to reparent the absolute paths in the .target file.
        if (!Module.IsPlugin ||
            !new DirectoryReference(Module.PluginDirectory).IsUnderDirectory(new DirectoryReference(Module.EngineDirectory)))
        {
            // The plugin is not located within the engine. Embed absolute paths and additional properties for UET to fix up
            // distributed builds where necessary.
            Console.WriteLine("EOS SDK (Android): Plugin is located outside of the engine, using absolute UPL paths.");
            foreach (var UPLFile in UPLFiles)
            {
                Module.AdditionalPropertiesForReceipt.Add("AndroidPlugin", Path.Combine(ModuleBase, UPLFile));
            }
            Module.AdditionalPropertiesForReceipt.Add("RedpointUETOriginalProjectDirectory", Target.ProjectFile?.Directory?.FullName ?? string.Empty);
        }
        else
        {
            // The plugin is embedded in the engine. Embed relative paths to the engine root, since "AdditionalPropertiesForReceipt"
            // supports this (and this is the way all engine plugins shipped by Epic work).
            Console.WriteLine("EOS SDK (Android): Plugin is located inside the engine, using relative UPL paths.");
            string ModuleBaseRelativeToEngine = Utils.MakePathRelativeTo(Module.ModuleDirectory, Target.RelativeEnginePath);
            foreach (var UPLFile in UPLFiles)
            {
                Module.AdditionalPropertiesForReceipt.Add("AndroidPlugin", Path.Combine(ModuleBaseRelativeToEngine, UPLFile));
            }
        }

        // Ensure required files and directories exist.
        RedpointEOSSDK.EnsureDirectoryExists(new[] {
            AndroidIncludeBase,
            AARDirectoryPath,
        });
        RedpointEOSSDK.EnsureFilesExist(new[] {
            Path.Combine(AndroidIncludeBase, "eos_version.h"),
            Path.Combine(ModuleBase, "RedpointEOSSDK_Android_UPL.xml"),
        });
        foreach (var UPLFile in UPLFiles)
        {
            RedpointEOSSDK.EnsureFilesExist(new[] { Path.Combine(ModuleBase, UPLFile) });
        }
    }

    public RedpointEOSSDKVersionConstraints GetSDKVersionConstraints()
    {
#if UE_5_1_OR_LATER
        return new RedpointEOSSDKVersionConstraints
        {
            SDKVersionDenyList = new[]
            {
                "20906061-v1.14.2-hf-2",
            }
        };
#else
        return default;
#endif
    }
}
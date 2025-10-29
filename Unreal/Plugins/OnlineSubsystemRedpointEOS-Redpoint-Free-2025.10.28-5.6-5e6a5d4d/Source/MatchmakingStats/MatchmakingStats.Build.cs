// Copyright June Rhodes. All Rights Reserved.

using UnrealBuildTool;
using System.IO;

public class MatchmakingStats : ModuleRules
{
	public MatchmakingStats(ReadOnlyTargetRules Target) : base(Target)
	{
		DefaultBuildSettings = BuildSettingsVersion.Latest;
		IncludeOrderVersion = EngineIncludeOrderVersion.Latest;
		Type = ModuleType.External;

		PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "Public"));
	}
}
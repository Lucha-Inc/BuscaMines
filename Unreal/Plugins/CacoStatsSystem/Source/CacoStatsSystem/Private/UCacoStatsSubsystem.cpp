#include "UCacoStatsSubsystem.h"
#include "Engine/Engine.h"

void UCacoStatsSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
    UE_LOG(LogTemp, Log, TEXT("UCacoStatsSubsystem initialized"));
}

void UCacoStatsSubsystem::Deinitialize()
{
    UE_LOG(LogTemp, Log, TEXT("UCacoStatsSubsystem deinitialized"));
}

void UCacoStatsSubsystem::SendStats(TScriptInterface<IStatsProvider> Provider)
{
    if (Provider)
    {
        TMap<FString, float> Stats = Provider->GetStats();
        for (auto& Pair : Stats)
        {
            UE_LOG(LogTemp, Log, TEXT("Stat: %s = %f"), *Pair.Key, Pair.Value);
        }
    }
}

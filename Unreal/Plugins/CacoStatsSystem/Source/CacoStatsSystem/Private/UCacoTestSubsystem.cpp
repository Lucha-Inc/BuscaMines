#include "UCacoTestSubsystem.h"
#include "Engine/Engine.h"

void UCacoTestSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
    UE_LOG(LogTemp, Log, TEXT("UCacoTestSubsystem initialized!"));
}

void UCacoTestSubsystem::Deinitialize()
{
    UE_LOG(LogTemp, Log, TEXT("UCacoTestSubsystem deinitialized!"));
}

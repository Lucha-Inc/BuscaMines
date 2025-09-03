#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "IStatsProvider.h"
#include "IStatsServiceInterface.h"
#include "UCacoStatsSubsystem.generated.h"

UCLASS()
class CACOSTATSSYSTEM_API UCacoStatsSubsystem : public UGameInstanceSubsystem, public IStatsServiceInterface
{
    GENERATED_BODY()

public:
    virtual void Initialize(FSubsystemCollectionBase& Collection) override;
    virtual void Deinitialize() override;

    virtual void SendStats(TScriptInterface<IStatsProvider> Provider) override;
};

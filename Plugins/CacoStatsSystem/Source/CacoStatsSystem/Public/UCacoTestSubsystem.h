#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "UCacoTestSubsystem.generated.h"

UCLASS()
class CACOSTATSSYSTEM_API UCacoTestSubsystem : public UGameInstanceSubsystem
{
    GENERATED_BODY()

public:
    // Inicialización del subsystem
    virtual void Initialize(FSubsystemCollectionBase& Collection) override;

    // Limpieza al finalizar
    virtual void Deinitialize() override;
};

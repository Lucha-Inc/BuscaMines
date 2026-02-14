// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "GISS_Master.generated.h"

/**
 * 
 */
UCLASS(Blueprintable)
class MINESWEEPER_02_API UGISS_Master : public UGameInstanceSubsystem
{
	GENERATED_BODY()
	
public:

    virtual void Initialize(FSubsystemCollectionBase& Collection) override;
    virtual void Deinitialize() override;

protected:

    UFUNCTION(BlueprintImplementableEvent, Category = "GISS_Master")
    void BP_OnInitialize();

    UFUNCTION(BlueprintImplementableEvent, Category = "GISS_Master")
    void BP_OnDeinitialize();
};
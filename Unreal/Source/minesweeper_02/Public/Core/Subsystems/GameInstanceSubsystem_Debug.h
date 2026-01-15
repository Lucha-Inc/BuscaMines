// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
//#include "Delegates/DelegateCombinations.h"
#include "UObject/ScriptDelegates.h"
#include "GameInstanceSubsystem_Debug.generated.h"


// Declaramos un **Event Dispatcher** para BP
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FDebugModeChangedSignature, bool, bIsActive);


/**
 * 
 */
UCLASS(Blueprintable)
class MINESWEEPER_02_API UGameInstanceSubsystem_Debug : public UGameInstanceSubsystem {
	GENERATED_BODY()

public:
	virtual void Initialize(FSubsystemCollectionBase& Collection) override;
	virtual void Deinitialize() override;
	// Aquí puedes agregar funciones que quieras exponer a BP más adelante

	UFUNCTION(BlueprintCallable, BlueprintPure, Category = "Debug")
	bool IsActive() const;

	/** Activa o desactiva el modo debug */
	UFUNCTION(BlueprintCallable, Category = "Debug")
	void SetActive(bool bActive);

	/** Event Dispatcher que se dispara cuando cambia el debug mode */
	UPROPERTY(BlueprintAssignable, Category = "Debug")
	FDebugModeChangedSignature OnDebugModeChanged;

protected:
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Debug")
	bool bDebugModeActive = false;
};



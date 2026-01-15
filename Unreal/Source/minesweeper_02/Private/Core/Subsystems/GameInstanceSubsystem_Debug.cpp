// Fill out your copyright notice in the Description page of Project Settings.


#include "Core/Subsystems/GameInstanceSubsystem_Debug.h"

void UGameInstanceSubsystem_Debug::Initialize(FSubsystemCollectionBase& Collection) {
	Super::Initialize(Collection);


	UE_LOG(LogTemp, Warning, TEXT("GameInstanceSubsystem inicializado"));
}

void UGameInstanceSubsystem_Debug::Deinitialize() {
	UE_LOG(LogTemp, Warning, TEXT("GameInstanceSubsystem destruido"));


	Super::Deinitialize();
}

bool UGameInstanceSubsystem_Debug::IsActive() const {
	return bDebugModeActive;
}

void UGameInstanceSubsystem_Debug::SetActive(bool bActive) {
	if (bDebugModeActive != bActive) {
		bDebugModeActive = bActive;

		// Disparar el Event Dispatcher
		OnDebugModeChanged.Broadcast(bDebugModeActive);
	}
}
